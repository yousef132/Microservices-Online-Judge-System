#!/usr/bin/env bash
# ------------------------
# CONFIG
# ------------------------
CODE_DIR="/code"
TIMEOUT_PER_TESTCASE=${1:-5}
SEPARATOR="---END---"
OUTPUT_SENTINEL="---DONE---"

USER_CPP="$CODE_DIR/main.cpp"
USER_PY="$CODE_DIR/main.py"
USER_CS="$CODE_DIR/main.csproj"
TESTCASES="$CODE_DIR/testcases.txt"
EXPECTED="$CODE_DIR/expected.txt"
OUTPUT_LOG="$CODE_DIR/output.txt"
ERROR_LOG="$CODE_DIR/error.txt"
RUNTIME_LOG="$CODE_DIR/runtime.txt"
TLE_LOG="$CODE_DIR/tle.txt"
FIFO_IN="$CODE_DIR/in.fifo"
FIFO_OUT="$CODE_DIR/out.fifo"

# ------------------------
# CLEAR OLD FILES
# ------------------------
: > "$OUTPUT_LOG"
: > "$ERROR_LOG"
: > "$RUNTIME_LOG"
: > "$TLE_LOG"

# ------------------------
# CLEANUP
# ------------------------
cleanup() {
    kill "$PID" 2>/dev/null
    rm -f "$FIFO_IN" "$FIFO_OUT"
}
trap cleanup EXIT

# ------------------------
# COMPILE / BUILD
# ------------------------
if [[ -f "$USER_CPP" ]]; then
    g++ -O2 -std=c++17 -o "$CODE_DIR/main.out" "$USER_CPP" 2> "$ERROR_LOG"
    if [[ $? -ne 0 ]]; then
        echo "COMPILATIONFAILED" >> "$ERROR_LOG"
        exit 1
    fi
    EXEC_CMD="stdbuf -o0 $CODE_DIR/main.out"

elif [[ -f "$USER_PY" ]]; then
    EXEC_CMD="python3 -u $USER_PY"

elif [[ -f "$USER_CS" ]]; then
    dotnet build "$USER_CS" -o "$CODE_DIR/output" 2>> "$ERROR_LOG"
    if [[ $? -ne 0 ]]; then
        echo "BUILDFAILED" >> "$ERROR_LOG"
        exit 1
    fi
    EXEC_CMD="DOTNET_UNBUFFERED_IO=1 dotnet $CODE_DIR/output/main.dll"

else
    echo "UNSUPPORTEDLANGUAGE" >> "$ERROR_LOG"
    exit 1
fi

# ------------------------
# FIFOS + LAUNCH PROGRAM ONCE
# ------------------------
mkfifo "$FIFO_IN" "$FIFO_OUT"

eval "$EXEC_CMD" < "$FIFO_IN" > "$FIFO_OUT" 2>> "$ERROR_LOG" &
PID=$!

exec 6> "$FIFO_IN"
exec 5< "$FIFO_OUT"

# ------------------------
# LOAD BLOCKS
# ------------------------
load_blocks() {
    local file="$1"
    local -n arr="$2"
    local block=""
    while IFS= read -r line || [[ -n "$line" ]]; do
        if [[ "$line" == "$SEPARATOR" ]]; then
            if [[ -n "$block" ]]; then
                arr+=("${block%$'\n'}")
            fi
            block=""
        else
            block+="$line"$'\n'
        fi
    done < "$file"
    if [[ -n "$block" ]]; then
        arr+=("${block%$'\n'}")
    fi
}

declare -a testcase_blocks
declare -a expected_blocks
load_blocks "$TESTCASES" testcase_blocks
load_blocks "$EXPECTED"  expected_blocks

# ------------------------
# VALIDATE
# ------------------------
if [[ ${#testcase_blocks[@]} -ne ${#expected_blocks[@]} ]]; then
    echo "TESTCASE_EXPECTED_MISMATCH: ${#testcase_blocks[@]} inputs vs ${#expected_blocks[@]} expected" >> "$ERROR_LOG"
    exit 1
fi

total=${#testcase_blocks[@]}

# ------------------------
# SEND T FIRST
# ------------------------
printf '%s\n' "$total" >&6

# ------------------------
# READ UNTIL SENTINEL
# Reads lines until ---DONE--- received or deadline hit
# ------------------------
read_until_sentinel() {
    local block=""
    local line
    local deadline=$(( $(date +%s%3N) + TIMEOUT_PER_TESTCASE * 1000 ))

    while true; do
        now=$(date +%s%3N)
        if (( now >= deadline )); then
            return 1
        fi

        remaining_ms=$(( deadline - now ))
        remaining_sec_int=$(( remaining_ms / 1000 ))
        remaining_sec_frac=$(( remaining_ms % 1000 ))
        printf -v remaining_sec "%d.%03d" "$remaining_sec_int" "$remaining_sec_frac"

        if IFS= read -u 5 -t "$remaining_sec" line; then
            if [[ "$line" == "$OUTPUT_SENTINEL" ]]; then
                break   # sentinel received — block complete
            fi
            block+="$line"$'\n'
        else
            return 1    # timeout without sentinel → TLE
        fi
    done

    printf '%s' "${block%$'\n'}"
    return 0
}

# ------------------------
# PROCESS TESTCASES
# ------------------------
PASSED=0

for (( i=0; i<total; i++ )); do
    input="${testcase_blocks[$i]}"
    expected="${expected_blocks[$i]}"

    printf '%s\n' "$input" >&6

    start_ms=$(date +%s%3N)
    actual=$(read_until_sentinel)
    status=$?
    end_ms=$(date +%s%3N)
    elapsed=$(( end_ms - start_ms ))

    # TLE
    if [[ $status -ne 0 ]]; then
        {
            echo "TIMELIMITEXCEEDED"
            echo "Testcase: $((i+1))"
            echo "Passed: $PASSED"
            echo "Time consumed (ms): $elapsed"
        } >> "$TLE_LOG"
        exit 1
    fi

    # Log actual output — same delimiter as input/expected
    {
        echo "$actual"
        echo "$SEPARATOR"
    } >> "$OUTPUT_LOG"

    # Wrong answer
    if [[ "$actual" != "$expected" ]]; then
        {
            echo "WRONG_ANSWER"
            echo "Testcase: $((i+1))"
            echo "Passed: $PASSED"
            echo "--- Expected ---"
            echo "$expected"
            echo "--- Got ---"
            echo "$actual"
        } >> "$RUNTIME_LOG"
        exit 1
    fi

    PASSED=$(( PASSED + 1 ))
    echo "Testcase $((i+1)) time: ${elapsed}ms" >> "$RUNTIME_LOG"
done

# ------------------------
# CLEAN EXIT
# ------------------------
exec 6>&-
wait "$PID" 2>/dev/null
exit_code=$?

if [[ $exit_code -ne 0 ]]; then
    {
        echo "RUNTIMEERROR"
        echo "Passed: $PASSED"
        echo "Exit code: $exit_code"
    } >> "$RUNTIME_LOG"
    exit 1
fi

echo "ACCEPTED" >> "$RUNTIME_LOG"