#!/usr/bin/env bash
# ------------------------
# CONFIG
# ------------------------
REQUEST_ID="$1"
TIMEOUT_PER_TESTCASE="${2:-5}"

if [[ -z "$REQUEST_ID" ]]; then
    echo "ERROR: REQUEST_ID is missing"
    exit 1
fi

CODE_DIR="/code/requests/$REQUEST_ID"
mkdir -p "$CODE_DIR"
cd "$CODE_DIR" || exit 1

VERDICT_DIR="."
SEPARATOR="---END---"
OUTPUT_SENTINEL="---DONE---"

USER_CPP="main.cpp"
USER_PY="main.py"
USER_CS="main.csproj"
TESTCASES="testcases.txt"
EXPECTED="expected.txt"
OUTPUT_LOG="output.txt"
ERROR_LOG="error.txt"
RUNTIME_LOG="runtime.txt"
TLE_LOG="tle.txt"
FIFO_IN="in.fifo"
FIFO_OUT="out.fifo"

# ------------------------
# CLEAR FILES
# ------------------------
: > "$OUTPUT_LOG"
: > "$ERROR_LOG"
: > "$RUNTIME_LOG"
: > "$TLE_LOG"

# ------------------------
# CLEANUP
# ------------------------
PID=""
cleanup_and_exit() {
    local exit_val=$1
    [[ -n "$PID" ]] && kill "$PID" 2>/dev/null && wait "$PID" 2>/dev/null
    exec 6>&- 2>/dev/null
    exec 5<&- 2>/dev/null
    rm -f "$FIFO_IN" "$FIFO_OUT"
    exit "$exit_val"
}
trap 'cleanup_and_exit 1' SIGTERM SIGINT

# ------------------------
# COMPILE / BUILD
# ------------------------
if [[ -f "$USER_CPP" ]]; then
    g++ -O2 -std=c++17 -o "$VERDICT_DIR/main.out" "$USER_CPP" 2> "$ERROR_LOG"
    if [[ $? -ne 0 ]]; then
        echo "COMPILATIONFAILED" >> "$ERROR_LOG"
        cleanup_and_exit 1
    fi
    EXEC_CMD="stdbuf -o0 $VERDICT_DIR/main.out"

elif [[ -f "$USER_PY" ]]; then
    EXEC_CMD="python3 -u $USER_PY"

elif [[ -f "$USER_CS" ]]; then
    dotnet build "$USER_CS" -o "$VERDICT_DIR/output" 2>> "$ERROR_LOG"
    if [[ $? -ne 0 ]]; then
        echo "BUILDFAILED" >> "$ERROR_LOG"
        cleanup_and_exit 1
    fi
    EXEC_CMD="DOTNET_UNBUFFERED_IO=1 dotnet $VERDICT_DIR/output/main.dll"

else
    echo "UNSUPPORTEDLANGUAGE" >> "$ERROR_LOG"
    cleanup_and_exit 1
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
    [[ -n "$block" ]] && arr+=("${block%$'\n'}")
}

declare -a testcase_blocks
declare -a expected_blocks
load_blocks "$TESTCASES" testcase_blocks
load_blocks "$EXPECTED"  expected_blocks

if [[ ${#testcase_blocks[@]} -ne ${#expected_blocks[@]} ]]; then
    echo "TESTCASE_EXPECTED_MISMATCH" >> "$ERROR_LOG"
    cleanup_and_exit 1
fi

total=${#testcase_blocks[@]}
printf '%s\n' "$total" >&6

# ------------------------
# READ UNTIL SENTINEL
# ------------------------
read_until_sentinel() {
    local block=""
    local line
    local deadline=$(( $(date +%s%3N) + TIMEOUT_PER_TESTCASE * 1000 ))

    while true; do
        now=$(date +%s%3N)
        (( now >= deadline )) && return 1

        remaining_ms=$(( deadline - now ))
        printf -v remaining_sec "%d.%03d" \
            $(( remaining_ms / 1000 )) \
            $(( remaining_ms % 1000 ))

        if IFS= read -u 5 -t "$remaining_sec" line; then
            [[ "$line" == "$OUTPUT_SENTINEL" ]] && break
            block+="$line"$'\n'
        else
            return 1
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
    if [[ $status -ne 0 ]] || (( elapsed > TIMEOUT_PER_TESTCASE * 1000 )); then
        {
            echo "TIMELIMITEXCEEDED"
            echo "Testcase: $((i+1))"
            echo "Passed: $PASSED"
            echo "Time consumed (ms): $elapsed"
            echo "Time limit (ms): $(( TIMEOUT_PER_TESTCASE * 1000 ))"
        } >> "$TLE_LOG"
        cleanup_and_exit 1
    fi

    # Log output
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
        cleanup_and_exit 1
    fi

    PASSED=$(( PASSED + 1 ))
    echo "Testcase $((i+1)) time: ${elapsed}ms" >> "$RUNTIME_LOG"
done

# ------------------------
# FINAL VERDICT
# ------------------------
wait "$PID" 2>/dev/null
exit_code=$?

if [[ $exit_code -ne 0 ]]; then
    {
        echo "RUNTIMEERROR"
        echo "Passed: $PASSED"
        echo "Exit code: $exit_code"
    } >> "$RUNTIME_LOG"
    cleanup_and_exit 1
fi

echo "ACCEPTED" >> "$RUNTIME_LOG"
cleanup_and_exit 0