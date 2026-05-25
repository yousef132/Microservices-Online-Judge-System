import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  BrowserRouter,
  Link,
  NavLink,
  Route,
  Routes,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import {
  ArrowDown,
  ArrowUp,
  Bookmark,
  BookmarkCheck,
  BookOpen,
  Check,
  ChevronLeft,
  ChevronRight,
  Clock,
  Edit3,
  Eye,
  FileText,
  Hash,
  Home,
  Image,
  KeyRound,
  Loader2,
  MessageSquare,
  Plus,
  RefreshCcw,
  Save,
  Search,
  Send,
  Sparkles,
  Trash2,
  Upload,
  User,
  X,
} from "lucide-react";
import "./App.css";

const API_BASE =
  import.meta.env.VITE_COMMUNITY_API_URL?.replace(/\/$/, "") || "/community-api";

const GET_DEDUPE_WINDOW_MS = 500;
const IN_FLIGHT_GET_REQUESTS = new Map();

const ENDPOINTS = [
  { method: "GET", path: "/api/articles", auth: false, label: "List articles" },
  { method: "GET", path: "/api/articles/{slug}", auth: false, label: "Get article by slug" },
  { method: "POST", path: "/api/articles", auth: true, label: "Create article" },
  { method: "PUT", path: "/api/articles/{articleId}", auth: true, label: "Update article" },
  { method: "DELETE", path: "/api/articles/{articleId}", auth: true, label: "Delete article" },
  { method: "GET", path: "/api/articles/me", auth: true, label: "My articles" },
  {
    method: "POST",
    path: "/api/articles/{articleId}/cover-image-upload-url",
    auth: true,
    label: "Generate cover upload URL",
  },
  {
    method: "GET",
    path: "/api/articles/{articleId}/comments",
    auth: false,
    label: "List comments",
  },
  {
    method: "POST",
    path: "/api/articles/{articleId}/comments",
    auth: true,
    label: "Create comment",
  },
  {
    method: "PUT",
    path: "/api/articles/{articleId}/comments/{commentId}",
    auth: true,
    label: "Update comment",
  },
  {
    method: "DELETE",
    path: "/api/articles/{articleId}/comments/{commentId}",
    auth: true,
    label: "Delete comment",
  },
  { method: "POST", path: "/api/votes", auth: true, label: "Cast vote" },
  { method: "GET", path: "/api/bookmarks", auth: true, label: "Bookmarked articles" },
  { method: "POST", path: "/api/bookmarks", auth: true, label: "Add bookmark" },
  { method: "DELETE", path: "/api/bookmarks/{articleId}", auth: true, label: "Remove bookmark" },
  { method: "GET", path: "/api/recommendations", auth: true, label: "Recommendations" },
];

const DEFAULT_ARTICLE_FORM = {
  title: "",
  body: "",
  tags: "",
  status: "Draft",
  coverImageKey: "",
};

const SORT_OPTIONS = [
  { value: "new", label: "Newest" },
  { value: "hot", label: "Hot" },
  { value: "top", label: "Top voted" },
];

function readToken() {
  return localStorage.getItem("communityToken") || "";
}

function getField(value, key, fallback = undefined) {
  if (!value || typeof value !== "object") return fallback;
  const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
  return value[key] ?? value[pascalKey] ?? fallback;
}

function asItems(page) {
  return getField(page, "items", []);
}

function formatDate(value) {
  if (!value) return "Not published";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Unknown date";
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(date);
}

function formatDateTime(value) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(date);
}

function normalizeTags(tags) {
  if (Array.isArray(tags)) return tags;
  return tags
    .split(",")
    .map((tag) => tag.trim().toLowerCase())
    .filter(Boolean);
}

function problemMessage(error) {
  if (!error) return "";
  if (typeof error === "string") return error;
  if (error.title) return error.title;
  if (error.message) return error.message;
  if (error.errors) {
    return Object.entries(error.errors)
      .map(([key, values]) => `${key}: ${Array.isArray(values) ? values.join(", ") : values}`)
      .join(" ");
  }
  return "Request failed.";
}

function useApi() {
  const [token, setTokenState] = useState(readToken);

  const setToken = useCallback((nextToken) => {
    setTokenState(nextToken);
    if (nextToken.trim()) localStorage.setItem("communityToken", nextToken.trim());
    else localStorage.removeItem("communityToken");
  }, []);

  const request = useCallback(
    async (path, options = {}) => {
      const method = (options.method || "GET").toUpperCase();
      const requestKey = method === "GET" ? `${token || "anonymous"}:${path}` : "";
      if (requestKey && IN_FLIGHT_GET_REQUESTS.has(requestKey)) {
        return IN_FLIGHT_GET_REQUESTS.get(requestKey);
      }

      const headers = new Headers(options.headers || {});
      if (options.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
      }
      if (token) headers.set("Authorization", `Bearer ${token}`);

      const requestPromise = (async () => {
        const response = await fetch(`${API_BASE}${path}`, {
          ...options,
          headers,
        });

        const contentType = response.headers.get("content-type") || "";
        const data = contentType.includes("application/json")
          ? await response.json()
          : await response.text();

        if (!response.ok) {
          const error = new Error(problemMessage(data) || `${response.status} ${response.statusText}`);
          error.status = response.status;
          error.details = data;
          throw error;
        }

        return data || null;
      })();

      if (requestKey) {
        IN_FLIGHT_GET_REQUESTS.set(requestKey, requestPromise);
        requestPromise.then(
          () => window.setTimeout(() => IN_FLIGHT_GET_REQUESTS.delete(requestKey), GET_DEDUPE_WINDOW_MS),
          () => window.setTimeout(() => IN_FLIGHT_GET_REQUESTS.delete(requestKey), GET_DEDUPE_WINDOW_MS),
        );
      }

      return requestPromise;
    },
    [token],
  );

  return useMemo(
    () => ({ request, token, setToken, isAuthenticated: Boolean(token) }),
    [request, setToken, token],
  );
}

function useDebouncedValue(value, delay = 350) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => setDebouncedValue(value), delay);
    return () => window.clearTimeout(timeoutId);
  }, [delay, value]);

  return debouncedValue;
}

function useResource(api, fetchFn, deps) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isEmpty, setIsEmpty] = useState(false);
  const apiRef = useRef(api);
  const fetchFnRef = useRef(fetchFn);

  apiRef.current = api;
  fetchFnRef.current = fetchFn;

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    setIsEmpty(false);
    setData(null);
    try {
      const result = await fetchFnRef.current(apiRef.current);
      setData(result);
    } catch (err) {
      if (err.status === 404) {
        setIsEmpty(true);
      } else {
        setError(err.message || "Request failed");
      }
    } finally {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  useEffect(() => {
    load();
  }, [load]);

  return { data, loading, error, isEmpty, reload: load };
}

class PageErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error) {
    console.error("Page render failed", error);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="error-boundary-fallback">
          <h1>Something went wrong</h1>
          <p>An unexpected error occurred. Please try reloading the page.</p>
          <button
            className="button primary"
            type="button"
            onClick={() => {
              this.setState({ hasError: false, error: null });
              window.location.reload();
            }}
          >
            <RefreshCcw size={16} />
            Reload page
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}

function Shell({ api }) {
  return (
    <div className="community-app">
      <Header api={api} />
      <PageErrorBoundary>
        <Routes>
          <Route path="/" element={<Dashboard api={api} />} />
          <Route path="/articles/new" element={<ArticleEditor api={api} mode="create" />} />
          <Route path="/articles/:slug" element={<ArticleDetails api={api} />} />
          <Route path="/articles/:slug/edit" element={<ArticleEditor api={api} mode="edit" />} />
          <Route path="/me" element={<ArticleCollection api={api} mode="mine" />} />
          <Route path="/bookmarks" element={<ArticleCollection api={api} mode="bookmarks" />} />
          <Route path="/recommendations" element={<Recommendations api={api} />} />
          <Route path="/endpoints" element={<EndpointWorkbench api={api} />} />
        </Routes>
      </PageErrorBoundary>
    </div>
  );
}

function Header({ api }) {
  const [draftToken, setDraftToken] = useState(api.token);
  const [tokenOpen, setTokenOpen] = useState(false);

  return (
    <header className="topbar">
      <Link className="brand" to="/">
        <BookOpen size={24} />
        <span>JudgeSync Community</span>
      </Link>

      <nav className="nav-links" aria-label="Community navigation">
        <NavLink to="/">
          <Home size={16} />
          Feed
        </NavLink>
        <NavLink to="/me">
          <FileText size={16} />
          Mine
        </NavLink>
        <NavLink to="/bookmarks">
          <Bookmark size={16} />
          Saved
        </NavLink>
        <NavLink to="/recommendations">
          <Sparkles size={16} />
          Picks
        </NavLink>
        <NavLink to="/endpoints">
          <Hash size={16} />
          Endpoints
        </NavLink>
      </nav>

      <div className="topbar-actions">
        <Link className="icon-button primary-action" to="/articles/new" title="Create article">
          <Plus size={18} />
          <span>Write</span>
        </Link>
        <button
          className="icon-button"
          type="button"
          onClick={() => {
            setDraftToken(api.token);
            setTokenOpen(true);
          }}
          title="Set bearer token"
        >
          <KeyRound size={18} />
          <span>{api.isAuthenticated ? "Token set" : "Token"}</span>
        </button>
      </div>

      {tokenOpen && (
        <div className="modal-backdrop" role="presentation">
          <section className="modal" role="dialog" aria-modal="true" aria-label="Bearer token">
            <div className="modal-header">
              <div>
                <p className="eyebrow">Authorization</p>
                <h2>Bearer token</h2>
              </div>
              <button className="ghost-icon" type="button" onClick={() => setTokenOpen(false)} title="Close">
                <X size={18} />
              </button>
            </div>
            <textarea
              className="token-input"
              value={draftToken}
              onChange={(event) => setDraftToken(event.target.value)}
              placeholder="Paste a JWT from your auth service"
              rows={6}
            />
            <div className="modal-actions">
              <button
                className="button subtle"
                type="button"
                onClick={() => {
                  setDraftToken("");
                  api.setToken("");
                }}
              >
                Clear
              </button>
              <button
                className="button primary"
                type="button"
                onClick={() => {
                  api.setToken(draftToken);
                  setTokenOpen(false);
                }}
              >
                <Check size={16} />
                Save token
              </button>
            </div>
          </section>
        </div>
      )}
    </header>
  );
}

function Dashboard({ api }) {
  const [searchParams, setSearchParams] = useSearchParams();
  const [articles, setArticles] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const recResource = useResource(
    api,
    (a) => (a.isAuthenticated ? a.request("/api/recommendations?limit=5") : Promise.resolve([])),
    [api, api.isAuthenticated],
  );

  const filters = {
    tag: searchParams.get("tag") || "",
    sort: searchParams.get("sort") || "new",
    page: Number(searchParams.get("page") || 1),
  };
  const debouncedTag = useDebouncedValue(filters.tag);

  const loadFeed = useCallback(async () => {
    setLoading(true);
    setError("");
    const query = new URLSearchParams({
      sort: filters.sort,
      page: String(filters.page),
      pageSize: "12",
    });
    if (debouncedTag) query.set("tag", debouncedTag);

    try {
      setArticles(await api.request(`/api/articles?${query}`));
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, debouncedTag, filters.page, filters.sort]);

  useEffect(() => {
    loadFeed();
  }, [loadFeed]);

  const updateFilter = (key, value) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    if (key !== "page") next.set("page", "1");
    setSearchParams(next);
  };

  const totalCount = getField(articles, "totalCount", 0);
  const pageSize = getField(articles, "pageSize", 12);
  const pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <main className="page-grid">
      <section className="content-column">
        <div className="hero-band">
          <div>
            <p className="eyebrow">Community system</p>
            <h1>Articles, discussions, bookmarks, votes, and recommendations.</h1>
          </div>
          <Link className="button primary" to="/articles/new">
            <Plus size={18} />
            New article
          </Link>
        </div>

        <div className="toolbar">
          <label className="search-field">
            <Search size={18} />
            <input
              value={filters.tag}
              onChange={(event) => updateFilter("tag", event.target.value)}
              placeholder="Filter by tag"
            />
          </label>
          <div className="segmented">
            {SORT_OPTIONS.map((option) => (
              <button
                className={filters.sort === option.value ? "active" : ""}
                key={option.value}
                type="button"
                onClick={() => updateFilter("sort", option.value)}
              >
                {option.label}
              </button>
            ))}
          </div>
          <button className="icon-button" type="button" onClick={loadFeed} title="Refresh feed">
            <RefreshCcw size={17} />
          </button>
        </div>

        <AsyncState loading={loading} error={error}>
          <ArticleList articles={asItems(articles)} />
          <Pagination
            page={filters.page}
            pageCount={pageCount}
            onPage={(page) => updateFilter("page", String(page))}
          />
        </AsyncState>
      </section>

      <aside className="side-column">
        <EndpointCoverage />
        <section className="panel-block">
          <div className="section-title">
            <Sparkles size={18} />
            <h2>Recommendations</h2>
          </div>
          {api.isAuthenticated ? (
            recResource.loading ? (
              <div className="loading-state">
                <Loader2 className="spin" size={22} />
                Loading
              </div>
            ) : recResource.isEmpty ? (
              <EmptyState
                icon={Sparkles}
                title="No recommendations"
                body="We don't have personalized picks for you yet."
              />
            ) : recResource.error ? (
              <EmptyState
                title="Could not load picks"
                body={recResource.error}
                action={{ label: "Retry", onClick: recResource.reload }}
              />
            ) : (
              <CompactArticleList
                articles={Array.isArray(recResource.data) ? recResource.data : []}
                emptyText="No recommendations yet."
              />
            )
          ) : (
            <EmptyState
              icon={Sparkles}
              title="Token required"
              body="Paste a bearer token to load personalized picks."
            />
          )}
        </section>
      </aside>
    </main>
  );
}

function ArticleCollection({ api, mode }) {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const title = mode === "mine" ? "My articles" : "Bookmarked articles";
  const path = mode === "mine" ? "/api/articles/me" : "/api/bookmarks";

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setResult(await api.request(`${path}?page=${page}&pageSize=12`));
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, page, path]);

  useEffect(() => {
    load();
  }, [load]);

  const totalCount = getField(result, "totalCount", 0);
  const pageSize = getField(result, "pageSize", 12);
  const pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <main className="single-column-page">
      <PageHeading
        eyebrow={mode === "mine" ? "Authenticated" : "Saved"}
        title={title}
        action={<button className="icon-button" type="button" onClick={load} title="Refresh"><RefreshCcw size={17} /></button>}
      />
      <AsyncState loading={loading} error={error}>
        <ArticleList articles={asItems(result)} />
        <Pagination page={page} pageCount={pageCount} onPage={setPage} />
      </AsyncState>
    </main>
  );
}

function Recommendations({ api }) {
  const [limit, setLimit] = useState(10);
  const debouncedLimit = useDebouncedValue(limit);
  const resource = useResource(
    api,
    (a) => a.request(`/api/recommendations?limit=${debouncedLimit}`),
    [api, debouncedLimit],
  );

  return (
    <main className="single-column-page">
      <PageHeading
        eyebrow="Personalized"
        title="Recommendations"
        action={
          <label className="small-control">
            Limit
            <input
              min="1"
              max="50"
              type="number"
              value={limit}
              onChange={(event) => setLimit(event.target.value)}
            />
          </label>
        }
      />
      {resource.loading ? (
        <div className="loading-state">
          <Loader2 className="spin" size={22} />
          Loading
        </div>
      ) : resource.isEmpty ? (
        <EmptyState
          icon={Sparkles}
          title="No recommendations"
          body="We don't have personalized picks for you yet."
        />
      ) : resource.error ? (
        <EmptyState
          icon={X}
          title="Could not load recommendations"
          body={resource.error}
          action={{ label: "Retry", onClick: resource.reload }}
        />
      ) : (
        <ArticleList articles={Array.isArray(resource.data) ? resource.data : []} />
      )}
    </main>
  );
}

function ArticleDetails({ api }) {
  const { slug } = useParams();
  const navigate = useNavigate();
  const [article, setArticle] = useState(null);
  const [commentBody, setCommentBody] = useState("");
  const [replyTo, setReplyTo] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState("");
  const [uploadState, setUploadState] = useState({ contentType: "image/png", result: null, error: "" });

  const articleId = getField(article, "id", "");
  const commentsResource = useResource(
    api,
    (a) => (articleId ? a.request(`/api/articles/${articleId}/comments`) : Promise.resolve([])),
    [api, articleId],
  );

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setArticle(await api.request(`/api/articles/${slug}`));
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, slug]);

  useEffect(() => {
    load();
  }, [load]);

  const vote = async (targetId, targetType, value) => {
    setBusy(`${targetType}-${targetId}-${value}`);
    try {
      const result = await api.request("/api/votes", {
        method: "POST",
        body: JSON.stringify({ targetId, targetType, value }),
      });
      if (targetType === "Article") {
        setArticle((current) => ({
          ...current,
          voteCount: getField(result, "newVoteCount", getField(current, "voteCount", 0)),
          userVote: getField(result, "userVote", value),
        }));
      } else {
        await commentsResource.reload();
      }
    } catch (voteError) {
      setError(voteError.message);
    } finally {
      setBusy("");
    }
  };

  const toggleBookmark = async () => {
    if (!articleId) return;
    setBusy("bookmark");
    try {
      const bookmarked = getField(article, "bookmarked", false);
      const result = await api.request(bookmarked ? `/api/bookmarks/${articleId}` : "/api/bookmarks", {
        method: bookmarked ? "DELETE" : "POST",
        body: bookmarked ? undefined : JSON.stringify({ articleId }),
      });
      setArticle((current) => ({ ...current, bookmarked: getField(result, "bookmarked", !bookmarked) }));
    } catch (bookmarkError) {
      setError(bookmarkError.message);
    } finally {
      setBusy("");
    }
  };

  const submitComment = async (event) => {
    event.preventDefault();
    if (!commentBody.trim() || !articleId) return;
    setBusy("comment");
    try {
      await api.request(`/api/articles/${articleId}/comments`, {
        method: "POST",
        body: JSON.stringify({ body: commentBody, parentCommentId: replyTo }),
      });
      setCommentBody("");
      setReplyTo(null);
      await commentsResource.reload();
    } catch (commentError) {
      setError(commentError.message);
    } finally {
      setBusy("");
    }
  };

  const deleteArticle = async () => {
    if (!articleId || !window.confirm("Delete this article?")) return;
    setBusy("delete-article");
    try {
      await api.request(`/api/articles/${articleId}`, { method: "DELETE" });
      navigate("/");
    } catch (deleteError) {
      setError(deleteError.message);
      setBusy("");
    }
  };

  const generateUploadUrl = async (event) => {
    event.preventDefault();
    if (!articleId) return;
    setBusy("cover-url");
    setUploadState((current) => ({ ...current, result: null, error: "" }));
    try {
      const result = await api.request(`/api/articles/${articleId}/cover-image-upload-url`, {
        method: "POST",
        body: JSON.stringify({ contentType: uploadState.contentType }),
      });
      setUploadState((current) => ({ ...current, result }));
    } catch (uploadError) {
      setUploadState((current) => ({ ...current, error: uploadError.message }));
    } finally {
      setBusy("");
    }
  };

  const removeComment = async (commentId) => {
    if (!articleId || !window.confirm("Delete this comment?")) return;
    setBusy(`delete-${commentId}`);
    try {
      await api.request(`/api/articles/${articleId}/comments/${commentId}`, { method: "DELETE" });
      await commentsResource.reload();
    } catch (deleteError) {
      setError(deleteError.message);
    } finally {
      setBusy("");
    }
  };

  return (
    <main className="article-page">
      <AsyncState loading={loading} error={error}>
        {article && (
          <>
            <article className="article-detail">
              {getField(article, "coverImageUrl") && (
                <img className="cover-image" src={getField(article, "coverImageUrl")} alt="" />
              )}
              <div className="article-meta">
                <span>{getField(article, "status", "Draft")}</span>
                <span>{formatDate(getField(article, "publishedAt") || getField(article, "createdAt"))}</span>
              </div>
              <h1>{getField(article, "title", "Untitled article")}</h1>
              <AuthorLine article={article} />
              <TagRow tags={getField(article, "tags", [])} />
              <p className="article-body">{getField(article, "body", "")}</p>
              <div className="article-actions">
                <VoteControl
                  count={getField(article, "voteCount", 0)}
                  userVote={getField(article, "userVote", 0)}
                  onVote={(value) => vote(articleId, "Article", value)}
                  busy={busy.startsWith("Article")}
                />
                <button className="button subtle" type="button" onClick={toggleBookmark} disabled={busy === "bookmark"}>
                  {getField(article, "bookmarked", false) ? <BookmarkCheck size={17} /> : <Bookmark size={17} />}
                  {getField(article, "bookmarked", false) ? "Saved" : "Save"}
                </button>
                <Link className="button subtle" to={`/articles/${slug}/edit`}>
                  <Edit3 size={17} />
                  Edit
                </Link>
                <button className="button danger" type="button" onClick={deleteArticle} disabled={busy === "delete-article"}>
                  <Trash2 size={17} />
                  Delete
                </button>
              </div>
            </article>

            <section className="detail-grid">
              <div className="panel-block">
                <div className="section-title">
                  <MessageSquare size={18} />
                  <h2>Comments</h2>
                </div>
                <form className="comment-form" onSubmit={submitComment}>
                  {replyTo && (
                    <button className="reply-pill" type="button" onClick={() => setReplyTo(null)}>
                      Replying to {replyTo.slice(0, 8)}
                      <X size={14} />
                    </button>
                  )}
                  <textarea
                    value={commentBody}
                    onChange={(event) => setCommentBody(event.target.value)}
                    placeholder="Add to the discussion"
                    rows={4}
                  />
                <button className="button primary" type="submit" disabled={busy === "comment"}>
                  <Send size={16} />
                  Comment
                </button>
                </form>
                {commentsResource.loading ? (
                  <div className="loading-state">
                    <Loader2 className="spin" size={22} />
                    Loading comments
                  </div>
                ) : commentsResource.isEmpty ? (
                  <EmptyState
                    icon={MessageSquare}
                    title="No comments"
                    body="Comments are not available for this article."
                  />
                ) : commentsResource.error ? (
                  <EmptyState
                    title="Could not load comments"
                    body={commentsResource.error}
                    action={{ label: "Retry", onClick: commentsResource.reload }}
                  />
                ) : (
                  <CommentTree
                    comments={Array.isArray(commentsResource.data) ? commentsResource.data : []}
                    busy={busy}
                    onReply={setReplyTo}
                    onDelete={removeComment}
                    onVote={(commentId, value) => vote(commentId, "Comment", value)}
                  />
                )}
              </div>

              <div className="panel-block">
                <div className="section-title">
                  <Image size={18} />
                  <h2>Cover upload</h2>
                </div>
                <form className="upload-form" onSubmit={generateUploadUrl}>
                  <label>
                    Content type
                    <input
                      value={uploadState.contentType}
                      onChange={(event) =>
                        setUploadState((current) => ({ ...current, contentType: event.target.value }))
                      }
                    />
                  </label>
                  <button className="button subtle" type="submit" disabled={busy === "cover-url"}>
                    <Upload size={16} />
                    Generate URL
                  </button>
                </form>
                {uploadState.error && <p className="error-text">{uploadState.error}</p>}
                {uploadState.result && (
                  <div className="upload-result">
                    <label>
                      Upload URL
                      <textarea readOnly rows={5} value={getField(uploadState.result, "uploadUrl", "")} />
                    </label>
                    <label>
                      Object key
                      <input readOnly value={getField(uploadState.result, "objectKey", "")} />
                    </label>
                  </div>
                )}
              </div>
            </section>
          </>
        )}
      </AsyncState>
    </main>
  );
}

function ArticleEditor({ api, mode }) {
  const { slug } = useParams();
  const navigate = useNavigate();
  const [articleId, setArticleId] = useState("");
  const [form, setForm] = useState(DEFAULT_ARTICLE_FORM);
  const [loading, setLoading] = useState(mode === "edit");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (mode !== "edit") return;
    let cancelled = false;
    setLoading(true);
    api
      .request(`/api/articles/${slug}`)
      .then((article) => {
        if (cancelled) return;
        setArticleId(getField(article, "id", ""));
        setForm({
          title: getField(article, "title", ""),
          body: getField(article, "body", ""),
          tags: getField(article, "tags", []).join(", "),
          status: getField(article, "status", "Draft"),
          coverImageKey: getField(article, "coverImageKey", ""),
        });
      })
      .catch((loadError) => setError(loadError.message))
      .finally(() => setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [api, mode, slug]);

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    const payload = {
      title: form.title,
      body: form.body,
      tags: normalizeTags(form.tags),
      status: form.status,
    };
    if (mode === "edit" && form.coverImageKey) payload.coverImageKey = form.coverImageKey;

    try {
      const result = await api.request(mode === "edit" ? `/api/articles/${articleId}` : "/api/articles", {
        method: mode === "edit" ? "PUT" : "POST",
        body: JSON.stringify(payload),
      });
      navigate(`/articles/${getField(result, "slug", slug)}`);
    } catch (saveError) {
      setError(saveError.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <main className="single-column-page narrow">
      <PageHeading
        eyebrow={mode === "edit" ? "Update article" : "Create article"}
        title={mode === "edit" ? "Edit article" : "Write an article"}
      />
      <AsyncState loading={loading} error={error}>
        <form className="editor-form" onSubmit={submit}>
          <label>
            Title
            <input
              required
              maxLength={200}
              value={form.title}
              onChange={(event) => setForm({ ...form, title: event.target.value })}
              placeholder="A clear title"
            />
          </label>
          <label>
            Body
            <textarea
              required
              maxLength={50000}
              rows={15}
              value={form.body}
              onChange={(event) => setForm({ ...form, body: event.target.value })}
              placeholder="Write the article content"
            />
          </label>
          <div className="form-row">
            <label>
              Tags
              <input
                required
                value={form.tags}
                onChange={(event) => setForm({ ...form, tags: event.target.value })}
                placeholder="dotnet, api, architecture"
              />
            </label>
            <label>
              Status
              <select value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value })}>
                <option value="Draft">Draft</option>
                <option value="Published">Published</option>
              </select>
            </label>
          </div>
          {mode === "edit" && (
            <label>
              Cover image key
              <input
                value={form.coverImageKey}
                onChange={(event) => setForm({ ...form, coverImageKey: event.target.value })}
                placeholder="Object key from generated upload URL"
              />
            </label>
          )}
          <button className="button primary" type="submit" disabled={saving}>
            <Save size={17} />
            {saving ? "Saving..." : "Save article"}
          </button>
        </form>
      </AsyncState>
    </main>
  );
}

function EndpointWorkbench({ api }) {
  const [selected, setSelected] = useState(ENDPOINTS[0]);
  const [pathValues, setPathValues] = useState({});
  const [query, setQuery] = useState("");
  const [body, setBody] = useState("");
  const [response, setResponse] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const pathParams = useMemo(() => [...selected.path.matchAll(/\{([^}]+)\}/g)].map((match) => match[1]), [selected]);
  const finalPath = selected.path.replace(/\{([^}]+)\}/g, (_, key) => pathValues[key] || `{${key}}`);

  const send = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError("");
    setResponse(null);
    try {
      const path = `${finalPath}${query.trim() ? `?${query.replace(/^\?/, "")}` : ""}`;
      const options = { method: selected.method };
      if (!["GET", "DELETE"].includes(selected.method) && body.trim()) options.body = body;
      const result = await api.request(path, options);
      setResponse(result);
    } catch (requestError) {
      setError(requestError.message);
      setResponse(requestError.details || null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="endpoint-page">
      <PageHeading eyebrow="Every mapped path" title="Endpoint workbench" />
      <div className="endpoint-layout">
        <section className="endpoint-list">
          {ENDPOINTS.map((endpoint) => (
            <button
              className={endpoint.path === selected.path && endpoint.method === selected.method ? "active" : ""}
              key={`${endpoint.method}-${endpoint.path}`}
              type="button"
              onClick={() => {
                setSelected(endpoint);
                setResponse(null);
                setError("");
              }}
            >
              <MethodBadge method={endpoint.method} />
              <span>{endpoint.label}</span>
              {endpoint.auth && <KeyRound size={14} />}
            </button>
          ))}
        </section>

        <form className="endpoint-client" onSubmit={send}>
          <div className="endpoint-path">
            <MethodBadge method={selected.method} />
            <code>{selected.path}</code>
          </div>
          {pathParams.map((param) => (
            <label key={param}>
              {param}
              <input
                value={pathValues[param] || ""}
                onChange={(event) => setPathValues({ ...pathValues, [param]: event.target.value })}
                placeholder={param}
              />
            </label>
          ))}
          <label>
            Query string
            <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="page=1&pageSize=20" />
          </label>
          {!["GET", "DELETE"].includes(selected.method) && (
            <label>
              JSON body
              <textarea
                rows={9}
                value={body}
                onChange={(event) => setBody(event.target.value)}
                placeholder='{"title":"Hello","body":"...","tags":["dotnet"],"status":"Published"}'
              />
            </label>
          )}
          <button className="button primary" type="submit" disabled={loading}>
            {loading ? <Loader2 className="spin" size={16} /> : <Send size={16} />}
            Send request
          </button>
          {error && <p className="error-text">{error}</p>}
          <pre className="response-box">{response ? JSON.stringify(response, null, 2) : "Response appears here."}</pre>
        </form>
      </div>
    </main>
  );
}

function ArticleList({ articles }) {
  if (!articles?.length) {
    return <EmptyState title="No articles found" body="Try a different filter or create the first article." />;
  }

  return (
    <div className="article-list">
      {articles.map((article) => (
        <ArticleCard article={article} key={getField(article, "id", getField(article, "slug"))} />
      ))}
    </div>
  );
}

function ArticleCard({ article }) {
  const slug = getField(article, "slug", "");
  return (
    <Link className="article-card" to={`/articles/${slug}`}>
      <div className="article-card-main">
        <div className="article-meta">
          <span>{getField(article, "status", "Draft")}</span>
          <span>{formatDate(getField(article, "publishedAt") || getField(article, "createdAt"))}</span>
        </div>
        <h2>{getField(article, "title", "Untitled article")}</h2>
        <p>{getField(article, "body", "").slice(0, 180)}</p>
        <TagRow tags={getField(article, "tags", [])} />
      </div>
      <div className="stats-row">
        <span>
          <ArrowUp size={15} />
          {getField(article, "voteCount", 0)}
        </span>
        <span>
          <MessageSquare size={15} />
          {getField(article, "commentCount", 0)}
        </span>
        <span>
          <Eye size={15} />
          {getField(article, "viewCount", 0)}
        </span>
      </div>
    </Link>
  );
}

function CompactArticleList({ articles, emptyText }) {
  if (!articles?.length) return <p className="muted">{emptyText}</p>;
  return (
    <div className="compact-list">
      {articles.map((article) => (
        <Link key={getField(article, "id", getField(article, "slug"))} to={`/articles/${getField(article, "slug")}`}>
          <strong>{getField(article, "title", "Untitled article")}</strong>
          <span>{getField(article, "voteCount", 0)} votes</span>
        </Link>
      ))}
    </div>
  );
}

function CommentTree({ comments, busy, onReply, onDelete, onVote }) {
  if (!comments?.length) return <p className="muted">No comments yet.</p>;
  return (
    <div className="comment-tree">
      {comments.map((comment) => (
        <CommentNode
          busy={busy}
          comment={comment}
          key={getField(comment, "id")}
          onDelete={onDelete}
          onReply={onReply}
          onVote={onVote}
        />
      ))}
    </div>
  );
}

function CommentNode({ comment, busy, onReply, onDelete, onVote }) {
  const commentId = getField(comment, "id", "");
  const replies = getField(comment, "replies", []);

  return (
    <div className="comment-node">
      <div className="comment-header">
        <span>
          <User size={14} />
          {getField(getField(comment, "author", {}), "username", "Unknown")}
        </span>
        <span>{formatDateTime(getField(comment, "createdAt"))}</span>
      </div>
      <p className={getField(comment, "isDeleted", false) ? "deleted-comment" : ""}>
        {getField(comment, "isDeleted", false) ? "Comment deleted" : getField(comment, "body", "")}
      </p>
      <div className="comment-actions">
        <VoteControl
          compact
          count={getField(comment, "voteCount", 0)}
          userVote={getField(comment, "userVote", 0)}
          onVote={(value) => onVote(commentId, value)}
          busy={busy.includes(commentId)}
        />
        <button className="text-button" type="button" onClick={() => onReply(commentId)}>
          Reply
        </button>
        <button className="text-button danger-text" type="button" onClick={() => onDelete(commentId)}>
          Delete
        </button>
      </div>
      {replies.length > 0 && (
        <div className="comment-replies">
          {replies.map((reply) => (
            <CommentNode
              busy={busy}
              comment={reply}
              key={getField(reply, "id")}
              onDelete={onDelete}
              onReply={onReply}
              onVote={onVote}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function VoteControl({ count, userVote, onVote, busy, compact = false }) {
  return (
    <div className={`vote-control ${compact ? "compact" : ""}`}>
      <button
        className={userVote === 1 ? "active" : ""}
        type="button"
        onClick={() => onVote(1)}
        disabled={busy}
        title="Upvote"
      >
        <ArrowUp size={compact ? 14 : 16} />
      </button>
      <span>{count}</span>
      <button
        className={userVote === -1 ? "active" : ""}
        type="button"
        onClick={() => onVote(-1)}
        disabled={busy}
        title="Downvote"
      >
        <ArrowDown size={compact ? 14 : 16} />
      </button>
    </div>
  );
}

function TagRow({ tags }) {
  if (!tags?.length) return null;
  return (
    <div className="tag-row">
      {tags.map((tag) => (
        <span key={tag}>{tag}</span>
      ))}
    </div>
  );
}

function AuthorLine({ article }) {
  const author = getField(article, "author", {});
  return (
    <div className="author-line">
      <User size={16} />
      <span>{getField(author, "username", "Unknown author")}</span>
      <Clock size={16} />
      <span>Updated {formatDate(getField(article, "updatedAt"))}</span>
    </div>
  );
}

function EndpointCoverage() {
  return (
    <section className="panel-block">
      <div className="section-title">
        <Hash size={18} />
        <h2>Endpoint coverage</h2>
      </div>
      <div className="endpoint-summary">
        <strong>{ENDPOINTS.length}</strong>
        <span>paths wired into the UI</span>
      </div>
      <Link className="button subtle full-width" to="/endpoints">
        Open workbench
      </Link>
    </section>
  );
}

function PageHeading({ eyebrow, title, action }) {
  return (
    <div className="page-heading">
      <div>
        <p className="eyebrow">{eyebrow}</p>
        <h1>{title}</h1>
      </div>
      {action}
    </div>
  );
}

function Pagination({ page, pageCount, onPage }) {
  return (
    <div className="pagination">
      <button className="icon-button" type="button" disabled={page <= 1} onClick={() => onPage(page - 1)}>
        <ChevronLeft size={17} />
      </button>
      <span>
        Page {page} of {pageCount}
      </span>
      <button className="icon-button" type="button" disabled={page >= pageCount} onClick={() => onPage(page + 1)}>
        <ChevronRight size={17} />
      </button>
    </div>
  );
}

function AsyncState({ loading, error, children }) {
  if (loading) {
    return (
      <div className="loading-state">
        <Loader2 className="spin" size={22} />
        Loading
      </div>
    );
  }
  if (error) {
    return <EmptyState icon={X} title="Request failed" body={error} />;
  }
  return children;
}

function EmptyState({ title, body, icon: Icon, action }) {
  return (
    <div className="empty-state">
      {Icon && <Icon className="empty-state-icon" size={48} />}
      <h2>{title}</h2>
      {body && <p>{body}</p>}
      {action && (
        <button className="button subtle" type="button" onClick={action.onClick}>
          {action.label}
        </button>
      )}
    </div>
  );
}

function MethodBadge({ method }) {
  return <span className={`method-badge ${method.toLowerCase()}`}>{method}</span>;
}

function App() {
  const api = useApi();

  return (
    <BrowserRouter>
      <Shell api={api} />
    </BrowserRouter>
  );
}

export default App;
