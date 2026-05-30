import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  BrowserRouter,
  Link,
  NavLink,
  Navigate,
  Route,
  Routes,
  useLocation,
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

function useAuth(api) {
  const navigate = useNavigate();
  const location = useLocation();
  const locationRef = useRef(location);

  useEffect(() => {
    locationRef.current = location;
  }, [location]);

  return useMemo(() => {
    const originalRequest = api.request;
    const wrappedRequest = async (path, options = {}) => {
      try {
        return await originalRequest(path, options);
      } catch (error) {
        if (error.status === 401) {
          api.setToken("");
          const currentUrl = locationRef.current.pathname + locationRef.current.search;
          navigate(`/auth?returnUrl=${encodeURIComponent(currentUrl)}`);
        }
        throw error;
      }
    };
    return { ...api, request: wrappedRequest };
  }, [api, navigate]);
}

function useNotificationCount(api) {
  const location = useLocation();
  const [count, setCount] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!api.isAuthenticated) {
      setCount(0);
      return;
    }
    let isMounted = true;
    setLoading(true);
    api.request("/api/notifications/unread-count")
      .then((data) => {
        if (isMounted) {
          setCount(data?.count || 0);
          setLoading(false);
        }
      })
      .catch(() => {
        if (isMounted) setLoading(false);
      });
    return () => {
      isMounted = false;
    };
  }, [api, location]);

  return { count, loading };
}

function usePresignedUpload(api) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const [progress, setProgress] = useState(0);

  const upload = useCallback(async (uploadUrlEndpoint, file) => {
    setUploading(true);
    setError("");
    setProgress(0);
    try {
      const response = await api.request(uploadUrlEndpoint, {
        method: "POST",
        body: JSON.stringify({ contentType: file.type })
      });
      
      const uploadUrl = response.uploadUrl || response.url;
      const key = response.key || response.fileKey || response.id;
      
      setProgress(50);
      const uploadRes = await fetch(uploadUrl, {
        method: "PUT",
        headers: { "Content-Type": file.type },
        body: file
      });
      
      if (!uploadRes.ok) {
        throw new Error("Failed to upload file to storage provider");
      }
      setProgress(100);
      return key;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setUploading(false);
    }
  }, [api]);

  return { upload, uploading, error, progress };
}

function RequireAuth({ api, children }) {
  const location = useLocation();
  if (!api.isAuthenticated) {
    return <Navigate to={`/auth?returnUrl=${encodeURIComponent(location.pathname + location.search)}`} replace />;
  }
  return children;
}

function RequireRole({ api, allowedRoles, children }) {
  const location = useLocation();
  
  const getRole = () => {
    if (!api.token) return null;
    try {
      const payloadBase64 = api.token.split(".")[1];
      if (!payloadBase64) return null;
      const payloadJson = atob(payloadBase64.replace(/-/g, "+").replace(/_/g, "/"));
      const payload = JSON.parse(payloadJson);
      return payload.role || payload.roles || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    } catch {
      return null;
    }
  };

  const userRole = getRole();
  const hasRole = Array.isArray(userRole) 
    ? allowedRoles.some(r => userRole.includes(r))
    : allowedRoles.includes(userRole);

  if (!api.isAuthenticated) {
    return <Navigate to={`/auth?returnUrl=${encodeURIComponent(location.pathname + location.search)}`} replace />;
  }

  if (!hasRole) {
    return (
      <div className="error-boundary-fallback">
        <h1>Access denied</h1>
        <p>You do not have permission to view this page.</p>
        <Link className="button primary" to="/">Return to Home</Link>
      </div>
    );
  }

  return children;
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
  const location = useLocation();
  const appApi = useAuth(api);
  const [searchOpen, setSearchOpen] = useState(false);
  const { count: unreadCount } = useNotificationCount(appApi);

  useEffect(() => {
    const handleKeyDown = (e) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        setSearchOpen(true);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  if (location.pathname === "/" && !appApi.isAuthenticated) {
    return <Navigate to="/welcome" replace />;
  }

  return (
    <div className="bg-background text-on-background min-h-screen flex flex-col md:flex-row font-body-md selection:bg-primary selection:text-on-primary antialiased">
      <aside className="hidden md:flex flex-col w-64 border-r border-white/5 bg-surface-dim h-screen sticky top-0">
        <div className="p-6">
          <Link className="flex items-center gap-2 mb-8 text-primary" to="/">
            <span className="material-symbols-outlined text-3xl">terminal</span>
            <span className="font-headline-md font-bold">DevStack</span>
          </Link>
          <nav className="space-y-2 flex flex-col">
            <NavLink to="/" className={({isActive}) => `flex items-center gap-3 px-3 py-2 rounded-lg font-label-md transition-colors ${isActive ? 'bg-surface-variant text-on-surface' : 'text-on-surface-variant hover:bg-surface-bright'}`}>
              <span className="material-symbols-outlined">home</span> Feed
            </NavLink>
            <NavLink to="/explore" className={({isActive}) => `flex items-center gap-3 px-3 py-2 rounded-lg font-label-md transition-colors ${isActive ? 'bg-surface-variant text-on-surface' : 'text-on-surface-variant hover:bg-surface-bright'}`}>
              <span className="material-symbols-outlined">explore</span> Explore
            </NavLink>
            <NavLink to="/tags" className={({isActive}) => `flex items-center gap-3 px-3 py-2 rounded-lg font-label-md transition-colors ${isActive ? 'bg-surface-variant text-on-surface' : 'text-on-surface-variant hover:bg-surface-bright'}`}>
              <span className="material-symbols-outlined">tag</span> Tags
            </NavLink>
            {appApi.isAuthenticated && (
              <>
                <NavLink to="/me" className={({isActive}) => `flex items-center gap-3 px-3 py-2 rounded-lg font-label-md transition-colors ${isActive ? 'bg-surface-variant text-on-surface' : 'text-on-surface-variant hover:bg-surface-bright'}`}>
                  <span className="material-symbols-outlined">description</span> My Articles
                </NavLink>
                <NavLink to="/bookmarks" className={({isActive}) => `flex items-center gap-3 px-3 py-2 rounded-lg font-label-md transition-colors ${isActive ? 'bg-surface-variant text-on-surface' : 'text-on-surface-variant hover:bg-surface-bright'}`}>
                  <span className="material-symbols-outlined">bookmark</span> Saved
                </NavLink>
              </>
            )}
          </nav>
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-h-screen">
        <Header api={appApi} onSearch={() => setSearchOpen(true)} unreadCount={unreadCount} />
        <main className="flex-1 overflow-y-auto">
          <PageErrorBoundary>
            <Routes>
              <Route path="/welcome" element={<WelcomePage />} />
              <Route path="/auth" element={<AuthPage api={appApi} />} />
              <Route path="/" element={<Dashboard api={appApi} />} />
              <Route path="/explore" element={<ExplorePage />} />
              <Route path="/tags" element={<TagsExplorerPage />} />
              <Route path="/search" element={<SearchResultsPage />} />
              
              <Route path="/notifications" element={<RequireAuth api={appApi}><NotificationsPage /></RequireAuth>} />
              <Route path="/communities" element={<RequireAuth api={appApi}><CommunitiesPage /></RequireAuth>} />
              <Route path="/communities/discover" element={<RequireAuth api={appApi}><DiscoverCommunitiesPage /></RequireAuth>} />
              <Route path="/feeds/custom" element={<RequireAuth api={appApi}><CustomFeedsPage /></RequireAuth>} />
              <Route path="/settings/profile" element={<RequireAuth api={appApi}><ProfileSettingsPage /></RequireAuth>} />
              <Route path="/settings/account" element={<RequireAuth api={appApi}><AccountSettingsPage /></RequireAuth>} />
              <Route path="/profile/:username/analytics" element={<RequireAuth api={appApi}><UserAnalyticsPage /></RequireAuth>} />
              
              <Route path="/analytics/detailed" element={<RequireRole api={appApi} allowedRoles={['Admin', 'SuperAdmin']}><DetailedAnalyticsPage /></RequireRole>} />
              <Route path="/admin" element={<RequireRole api={appApi} allowedRoles={['Admin', 'SuperAdmin']}><AdminDashboardPage /></RequireRole>} />
              <Route path="/admin/users" element={<RequireRole api={appApi} allowedRoles={['Admin', 'SuperAdmin']}><UserManagementPage /></RequireRole>} />
              <Route path="/moderation" element={<RequireRole api={appApi} allowedRoles={['Admin', 'SuperAdmin']}><ModerationQueuePage /></RequireRole>} />

              <Route path="/articles/new" element={<RequireAuth api={appApi}><ArticleEditor api={appApi} mode="create" /></RequireAuth>} />
              <Route path="/articles/:slug" element={<ArticleDetails api={appApi} />} />
              <Route path="/articles/:slug/edit" element={<RequireAuth api={appApi}><ArticleEditor api={appApi} mode="edit" /></RequireAuth>} />
              <Route path="/me" element={<RequireAuth api={appApi}><ArticleCollection api={appApi} mode="mine" /></RequireAuth>} />
              <Route path="/bookmarks" element={<RequireAuth api={appApi}><ArticleCollection api={appApi} mode="bookmarks" /></RequireAuth>} />
              </Routes>
          </PageErrorBoundary>
        </main>
      </div>
      {searchOpen && <SearchOverlay api={appApi} onClose={() => setSearchOpen(false)} />}
    </div>
  );
}

function Header({ api, onSearch, unreadCount }) {
  const navigate = useNavigate();

  return (
    <header className="sticky top-0 z-40 bg-surface-container/80 backdrop-blur-xl border-b border-white/10 flex justify-between items-center px-6 h-16">
      <div className="flex items-center gap-4 md:hidden">
        <Link to="/" className="text-primary font-bold">DevStack</Link>
      </div>
      <div className="flex-1"></div>
      <div className="flex items-center gap-4">
        <div className="hidden sm:flex relative items-center cursor-pointer" onClick={onSearch}>
          <span className="material-symbols-outlined absolute left-3 text-on-surface-variant pointer-events-none">search</span>
          <div className="bg-surface-dim border border-outline-variant rounded-full py-2 pl-10 pr-4 text-body-md text-on-surface-variant w-64 flex justify-between items-center">
            <span>Search...</span>
            <kbd className="font-label-sm text-xs px-1.5 bg-surface-variant rounded border border-white/10">⌘K</kbd>
          </div>
        </div>
        
        {api.isAuthenticated ? (
          <>
            <button className="relative text-on-surface-variant hover:bg-surface-bright/50 transition-colors p-2 rounded-full" onClick={() => navigate('/notifications')}>
              <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>notifications</span>
              {unreadCount > 0 && (
                <span className="absolute top-1 right-1 w-4 h-4 bg-primary text-on-primary text-[10px] flex items-center justify-center rounded-full font-bold">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </button>
            <div className="w-8 h-8 rounded-full bg-secondary-container/20 flex items-center justify-center text-secondary border border-secondary/20 cursor-pointer" onClick={() => { api.setToken(""); navigate('/'); }}>
              <span className="font-label-md text-label-md">ME</span>
            </div>
          </>
        ) : (
          <Link to="/auth" className="bg-primary text-on-primary font-label-md px-4 py-2 rounded-lg hover:bg-primary-fixed transition-colors active:scale-95">
            Log In
          </Link>
        )}
      </div>
    </header>
  );
}

function WelcomePage() {
  return (
    <main className="pt-32 pb-20 px-gutter max-w-container-max mx-auto bg-background text-on-background">
      <div className="text-center max-w-3xl mx-auto mb-16 relative">
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[400px] bg-primary/10 blur-[120px] rounded-full pointer-events-none -z-10"></div>
        <span className="inline-block py-1 px-3 rounded-full bg-surface-container border border-outline-variant font-label-sm text-label-sm text-tertiary mb-6">
          <span className="text-primary">v2.0</span> is now live
        </span>
        <h1 className="font-headline-xl text-headline-xl md:text-[64px] md:leading-[72px] mb-6 tracking-tight text-on-surface">
          The Workspace for <br/>
          <span className="text-transparent bg-clip-text bg-gradient-to-br from-primary to-tertiary">Modern Developers</span>
        </h1>
        <p className="font-body-lg text-body-lg text-on-surface-variant mb-10 max-w-2xl mx-auto">
          Connect, collaborate, and create with the smartest minds in tech. A high-fidelity community platform built for serious discussions, code sharing, and rapid problem solving.
        </p>
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
          <Link to="/auth" className="w-full sm:w-auto bg-primary text-on-primary font-label-md text-label-md px-8 py-4 rounded-xl hover:bg-primary-fixed transition-all active:scale-95 shadow-[0_0_20px_rgba(173,198,255,0.3)]">
            Join Now
          </Link>
          <Link to="/explore" className="w-full sm:w-auto bg-surface-variant/40 backdrop-blur-md border border-white/5 text-on-surface font-label-md text-label-md px-8 py-4 rounded-xl hover:bg-surface-bright/50 transition-all flex items-center justify-center gap-2">
            <span className="material-symbols-outlined">terminal</span>
            Explore API
          </Link>
        </div>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-24">
        <div className="bg-surface-variant/40 backdrop-blur-md border border-white/5 rounded-2xl p-8 col-span-1 md:col-span-2 relative overflow-hidden group hover:border-primary/30 transition-colors">
          <div className="absolute -right-20 -top-20 w-64 h-64 bg-secondary/10 blur-[80px] rounded-full pointer-events-none group-hover:bg-secondary/20 transition-all"></div>
          <div className="mb-6 h-12 w-12 rounded-lg bg-surface-container flex items-center justify-center border border-white/5">
            <span className="material-symbols-outlined text-tertiary text-3xl">forum</span>
          </div>
          <h3 className="font-headline-md text-headline-md text-on-surface mb-3">Deep Tech Discussions</h3>
          <p className="font-body-md text-body-md text-on-surface-variant mb-6 max-w-md">Threaded conversations designed for high information density. Share code snippets with syntax highlighting and inline execution context.</p>
        </div>
        
        <div className="bg-surface-variant/40 backdrop-blur-md border border-white/5 rounded-2xl p-8 col-span-1 relative overflow-hidden group hover:border-primary/30 transition-colors flex flex-col">
          <div className="absolute -right-10 -bottom-10 w-40 h-40 bg-primary/10 blur-[60px] rounded-full pointer-events-none group-hover:bg-primary/20 transition-all"></div>
          <div className="mb-6 h-12 w-12 rounded-lg bg-surface-container flex items-center justify-center border border-white/5">
            <span className="material-symbols-outlined text-primary text-3xl">article</span>
          </div>
          <h3 className="font-headline-md text-headline-md text-on-surface mb-3">Curated Blogs</h3>
          <p className="font-body-md text-body-md text-on-surface-variant flex-grow">Read and publish long-form technical articles with integrated reputation mechanics.</p>
        </div>
        
        <div className="bg-surface-variant/40 backdrop-blur-md border border-white/5 border-l-2 border-l-primary rounded-2xl p-8 col-span-1 md:col-span-3 flex flex-col md:flex-row items-center justify-between gap-8 group">
          <div className="max-w-xl">
            <div className="flex items-center gap-2 mb-4 text-secondary font-label-sm text-label-sm">
              <span className="material-symbols-outlined text-sm">trending_up</span>
              LIVE COMMUNITY STATS
            </div>
            <h3 className="font-headline-lg text-headline-lg text-on-surface mb-2">Growing Faster Than Ever</h3>
            <p className="font-body-md text-body-md text-on-surface-variant">Join thousands of developers solving complex problems every minute.</p>
          </div>
          <div className="flex gap-8 w-full md:w-auto">
            <div>
              <div className="font-headline-xl text-headline-xl text-primary mb-1">12K+</div>
              <div className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Active Users</div>
            </div>
            <div>
              <div className="font-headline-xl text-headline-xl text-tertiary mb-1">50M</div>
              <div className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Code Lines Shared</div>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}

function AuthPage({ api }) {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const returnUrl = searchParams.get("returnUrl") || "/";
  const [mode, setMode] = useState("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [username, setUsername] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const endpoint = mode === "login" ? "/api/users/login" : "/api/users/register";
      const body = mode === "login" ? { email, password } : { email, password, username };
      const data = await api.request(endpoint, {
        method: "POST",
        body: JSON.stringify(body)
      });
      if (data.token) {
        api.setToken(data.token);
        navigate(returnUrl);
      }
    } catch (err) {
      setError(err.message || "Authentication failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col md:flex-row min-h-screen w-full bg-background text-on-background">
      <div className="hidden md:flex md:w-1/2 lg:w-5/12 bg-surface-container-lowest relative overflow-hidden items-center justify-center p-gutter">
        <div className="relative z-10 max-w-md w-full flex flex-col gap-stack-lg bg-surface-variant/40 backdrop-blur-md border border-white/5 p-stack-lg rounded-xl">
          <div className="flex items-center gap-stack-sm">
            <span className="material-symbols-outlined text-primary text-4xl">terminal</span>
            <span className="font-headline-lg text-headline-lg text-on-surface tracking-tight">DevStack</span>
          </div>
          <div className="flex flex-col gap-stack-md">
            <h1 className="font-headline-xl text-headline-xl text-on-surface">The hub for serious developers.</h1>
            <p className="font-body-lg text-body-lg text-on-surface-variant">Join a high-signal community where engineers build, discuss, and scale tomorrow's infrastructure. No noise, just code.</p>
          </div>
        </div>
      </div>
      <div className="flex-1 flex items-center justify-center p-margin-mobile md:p-gutter bg-surface">
        <div className="w-full max-w-md flex flex-col gap-stack-lg">
          <div className="text-center md:text-left flex flex-col gap-stack-xs">
            <h2 className="font-headline-lg-mobile md:font-headline-lg text-on-surface">{mode === "login" ? "Welcome back" : "Create an account"}</h2>
            <p className="font-body-md text-on-surface-variant">{mode === "login" ? "Sign in to your account to continue." : "Join the DevStack community."}</p>
          </div>
          
          <div className="flex border-b border-outline-variant/30 mb-4">
            <button 
              className={`flex-1 py-2 text-center font-label-md border-b-2 ${mode === 'login' ? 'border-primary text-primary' : 'border-transparent text-on-surface-variant hover:text-on-surface'}`}
              onClick={() => setMode('login')}
              type="button"
            >
              Sign In
            </button>
            <button 
              className={`flex-1 py-2 text-center font-label-md border-b-2 ${mode === 'register' ? 'border-primary text-primary' : 'border-transparent text-on-surface-variant hover:text-on-surface'}`}
              onClick={() => setMode('register')}
              type="button"
            >
              Register
            </button>
          </div>

          {error && (
            <div className="p-3 bg-error-container/20 border border-error-container text-error rounded-md text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-stack-md">
            {mode === "register" && (
              <div className="flex flex-col gap-stack-xs">
                <label className="font-label-sm text-on-surface" htmlFor="username">Username</label>
                <input 
                  className="w-full bg-surface-dim border border-outline-variant rounded-DEFAULT px-4 py-3 text-on-surface focus:border-primary focus:ring-1 focus:ring-primary outline-none" 
                  id="username" 
                  value={username} 
                  onChange={(e) => setUsername(e.target.value)} 
                  required 
                  type="text" 
                />
              </div>
            )}
            <div className="flex flex-col gap-stack-xs">
              <label className="font-label-sm text-on-surface" htmlFor="email">Email Address</label>
              <input 
                className="w-full bg-surface-dim border border-outline-variant rounded-DEFAULT px-4 py-3 text-on-surface focus:border-primary focus:ring-1 focus:ring-primary outline-none" 
                id="email" 
                value={email} 
                onChange={(e) => setEmail(e.target.value)} 
                required 
                type="email" 
              />
            </div>
            <div className="flex flex-col gap-stack-xs">
              <label className="font-label-sm text-on-surface" htmlFor="password">Password</label>
              <input 
                className="w-full bg-surface-dim border border-outline-variant rounded-DEFAULT px-4 py-3 text-on-surface focus:border-primary focus:ring-1 focus:ring-primary outline-none" 
                id="password" 
                value={password} 
                onChange={(e) => setPassword(e.target.value)} 
                required 
                type="password" 
              />
            </div>
            <button 
              className="w-full flex justify-center py-3 px-4 mt-stack-sm rounded-DEFAULT text-on-primary bg-primary hover:bg-primary-container font-label-md transition-colors disabled:opacity-50" 
              type="submit"
              disabled={loading}
            >
              {loading ? "Please wait..." : (mode === "login" ? "Sign In" : "Register")}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

function SearchOverlay({ api, onClose }) {
  const [query, setQuery] = useState("");
  const debouncedQuery = useDebouncedValue(query, 300);
  const [results, setResults] = useState({ articles: [], tags: [], authors: [] });
  const navigate = useNavigate();

  useEffect(() => {
    if (!debouncedQuery) {
      setResults({ articles: [], tags: [], authors: [] });
      return;
    }
    api.request(`/api/search/suggestions?q=${encodeURIComponent(debouncedQuery)}`)
      .then(res => setResults(res || { articles: [], tags: [], authors: [] }))
      .catch(() => setResults({ articles: [], tags: [], authors: [] }));
  }, [debouncedQuery, api]);

  useEffect(() => {
    const handleEsc = (e) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handleEsc);
    return () => window.removeEventListener('keydown', handleEsc);
  }, [onClose]);

  const handleSelect = (url) => {
    navigate(url);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 bg-background/80 backdrop-blur-md flex items-start justify-center pt-24 px-4 sm:px-6">
      <div className="w-full max-w-2xl bg-surface-container-high rounded-xl border border-white/10 shadow-[0_10px_30px_rgba(0,0,0,0.5)] overflow-hidden flex flex-col">
        <div className="relative flex items-center px-4 py-4 border-b border-white/10 bg-surface-container group">
          <span className="material-symbols-outlined text-on-surface-variant ml-2 mr-3 group-focus-within:text-primary transition-colors">search</span>
          <input 
            autoFocus 
            className="w-full bg-transparent border-none text-on-surface font-body-lg focus:ring-0 focus:outline-none placeholder:text-on-surface-variant/50" 
            placeholder="Search communities, tags, users..." 
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
          <div className="flex items-center gap-2 ml-4 cursor-pointer" onClick={onClose}>
            <kbd className="font-label-sm text-label-sm px-2 py-1 bg-surface-variant rounded border border-white/10 text-on-surface-variant">Esc</kbd>
          </div>
        </div>
        
        <div className="max-h-[614px] overflow-y-auto overscroll-contain p-2">
          {(!query) ? (
            <div className="p-4 text-center text-on-surface-variant font-body-md">
              Start typing to search...
            </div>
          ) : (
            <>
              {results.authors?.length > 0 && (
                <div className="mb-4">
                  <h3 className="px-2 py-2 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Authors</h3>
                  {results.authors.map(author => (
                    <button key={author.id} onClick={() => handleSelect(`/profile/${author.username}`)} className="w-full flex flex-col px-3 py-3 rounded-lg hover:bg-surface-bright transition-colors group text-left mb-1 relative overflow-hidden">
                      <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-secondary opacity-0 group-hover:opacity-100 transition-opacity"></div>
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-secondary-container/20 flex items-center justify-center text-secondary border border-secondary/20">
                          <span className="font-label-md text-label-md">{author.username?.substring(0,2).toUpperCase()}</span>
                        </div>
                        <div>
                          <h4 className="font-label-md text-label-md text-on-surface group-hover:text-secondary transition-colors">{author.displayName || author.username}</h4>
                          <p className="font-label-sm text-label-sm text-on-surface-variant">@{author.username}</p>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
              {results.tags?.length > 0 && (
                <div className="mb-4">
                  <h3 className="px-2 py-2 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Tags</h3>
                  {results.tags.map(tag => (
                    <button key={tag.id} onClick={() => handleSelect(`/tags/${tag.name}`)} className="w-full flex flex-col px-3 py-3 rounded-lg hover:bg-surface-bright transition-colors group text-left mb-1 relative overflow-hidden">
                      <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-primary opacity-0 group-hover:opacity-100 transition-opacity"></div>
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded bg-primary-container/20 flex items-center justify-center text-primary">
                          <span className="material-symbols-outlined text-[18px]">tag</span>
                        </div>
                        <div>
                          <h4 className="font-label-md text-label-md text-on-surface group-hover:text-primary transition-colors">{tag.name}</h4>
                          <p className="font-label-sm text-label-sm text-on-surface-variant">{tag.postCount || 0} posts</p>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
              {results.articles?.length > 0 && (
                <div className="mb-4">
                  <h3 className="px-2 py-2 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Articles</h3>
                  {results.articles.map(article => (
                    <button key={article.id} onClick={() => handleSelect(`/articles/${article.slug}`)} className="w-full flex flex-col px-3 py-3 rounded-lg hover:bg-surface-bright transition-colors group text-left mb-1 relative overflow-hidden">
                      <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-tertiary opacity-0 group-hover:opacity-100 transition-opacity"></div>
                      <div className="flex items-start gap-3">
                        <span className="material-symbols-outlined text-tertiary mt-0.5 text-[20px]">article</span>
                        <div>
                          <h4 className="font-body-md text-body-md text-on-surface mb-1 line-clamp-1 group-hover:text-tertiary transition-colors">{article.title}</h4>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
              {results.articles?.length === 0 && results.tags?.length === 0 && results.authors?.length === 0 && (
                <div className="p-4 text-center text-on-surface-variant font-body-md">
                  No results found.
                </div>
              )}
            </>
          )}
        </div>
        
        <div className="px-4 py-3 bg-surface-container-lowest border-t border-white/5 flex items-center justify-between font-label-sm text-label-sm text-on-surface-variant">
          <div className="flex items-center gap-4">
            <span className="flex items-center gap-1.5">
              <kbd className="px-1.5 py-0.5 bg-surface-variant rounded border border-white/10 text-on-surface">Esc</kbd>
              to close
            </span>
          </div>
          <div>
            <span className="flex items-center gap-1.5">
              Search by <span className="font-bold text-on-surface">DevStack</span>
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}

function ExplorePage() {
  const { request } = useApi();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/explore/highlights").then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  if (loading) return <div className="p-8 text-on-surface-variant flex items-center gap-2"><Loader2 className="animate-spin" /> Loading explore...</div>;
  if (!data) return <div className="p-8 text-error">Failed to load explore data.</div>;

  return (
    <div className="p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full pb-24 md:pb-gutter">
      <div className="mb-stack-lg flex flex-col md:flex-row md:items-end justify-between gap-4">
        <div>
          <h1 className="font-headline-xl text-headline-xl text-on-surface mb-2 tracking-tight">Discovery Hub</h1>
          <p className="font-body-lg text-body-lg text-on-surface-variant max-w-2xl">Find new communities, trending technical topics, and top contributors pushing the boundaries.</p>
        </div>
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-stack-md lg:gap-6 auto-rows-[minmax(180px,auto)]">
        <section className="lg:col-span-8 bg-surface-container/40 backdrop-blur-md border border-white/5 rounded-xl p-6 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-64 h-64 bg-primary/10 rounded-full blur-[80px] -translate-y-1/2 translate-x-1/3 pointer-events-none"></div>
          <div className="flex items-center justify-between mb-6 relative z-10">
            <h2 className="font-headline-md text-[20px] font-bold text-on-surface flex items-center gap-2">
              <span className="material-symbols-outlined text-tertiary">trending_up</span>
              Trending Ecosystems
            </h2>
            <Link to="/tags" className="text-primary font-label-md text-label-md hover:underline decoration-primary/50 underline-offset-4">View All</Link>
          </div>
          <div className="flex flex-wrap gap-3 relative z-10">
            {data.trendingTags?.map(tag => (
              <Link key={tag.name} to={`/search?q=${tag.name}&category=articles`} className="px-4 py-2 bg-surface border border-white/5 rounded-full flex items-center gap-2 hover:border-primary/50 hover:bg-primary/5 transition-all cursor-pointer">
                <span className="font-label-md text-label-md text-on-surface">#{tag.name}</span>
                <span className="font-label-sm text-[10px] text-primary bg-primary/10 px-1.5 py-0.5 rounded flex items-center gap-0.5"><span className="material-symbols-outlined text-[10px]">arrow_upward</span>{tag.postCount}</span>
              </Link>
            ))}
          </div>
        </section>
        
        <section className="lg:col-span-4 bg-surface-container/40 backdrop-blur-md border border-white/5 rounded-xl p-6 row-span-2 flex flex-col relative overflow-hidden">
          <div className="flex items-center justify-between mb-6 relative z-10">
            <h2 className="font-headline-md text-[18px] font-bold text-on-surface flex items-center gap-2">
              <span className="material-symbols-outlined text-secondary">workspace_premium</span>
              Featured Articles
            </h2>
          </div>
          <div className="space-y-4 flex-1 relative z-10">
            {data.featuredArticles?.map((article, i) => (
              <Link key={article.slug} to={`/articles/${article.slug}`} className="flex items-center justify-between p-3 rounded-lg hover:bg-surface-container-high/50 transition-colors border border-transparent hover:border-white/5">
                <div className="flex items-center gap-3">
                  <div className="relative">
                    <div className="w-10 h-10 rounded-full bg-surface-container-highest border border-white/10 flex items-center justify-center font-label-md text-on-surface">
                      {article.authorName[0]?.toUpperCase()}
                    </div>
                    <div className="absolute -bottom-1 -right-1 bg-surface-container-highest text-on-surface font-label-sm text-[9px] w-4 h-4 flex items-center justify-center rounded-full font-bold border border-background">{i + 1}</div>
                  </div>
                  <div className="flex flex-col">
                    <p className="font-label-md text-label-md text-on-surface leading-tight line-clamp-1">{article.title}</p>
                    <p className="font-label-sm text-label-sm text-on-surface-variant opacity-70">by {article.authorName}</p>
                  </div>
                </div>
                <div className="text-right shrink-0 ml-2">
                  <span className="font-label-md text-label-md text-on-surface-variant px-2 py-0.5 bg-surface-container rounded-md">{article.voteCount} pts</span>
                </div>
              </Link>
            ))}
          </div>
        </section>
        
        <section className="lg:col-span-8 flex flex-col gap-4">
          <div className="flex items-center justify-between mt-2 px-1">
            <h2 className="font-headline-md text-[20px] font-bold text-on-surface flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">hub</span>
              Active Communities
            </h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {data.activeCommunities?.map(community => (
              <div key={community.slug} className="bg-surface-container/40 backdrop-blur-md border border-white/5 hover:bg-surface-container-high/60 rounded-xl p-5 transition-all duration-300 flex flex-col h-full cursor-pointer relative overflow-hidden group">
                <div className="flex justify-between items-start mb-4 relative z-10">
                  <div className="w-12 h-12 rounded-lg bg-surface-container-highest border border-white/10 flex items-center justify-center text-2xl shadow-inner">
                    {community.name[0]?.toUpperCase()}
                  </div>
                  <button className="bg-primary/10 text-primary border border-primary/20 hover:bg-primary hover:text-on-primary font-label-md text-label-sm px-3 py-1.5 rounded-full transition-colors font-bold tracking-wide">
                    Join
                  </button>
                </div>
                <h3 className="font-headline-md text-[18px] font-semibold text-on-surface mb-1 relative z-10">{community.name}</h3>
                <div className="mt-auto flex items-center gap-4 text-on-surface-variant font-label-sm text-label-sm border-t border-white/5 pt-3 relative z-10">
                  <div className="flex items-center gap-1">
                    <span className="material-symbols-outlined text-[14px]">group</span>
                    {community.memberCount} members
                  </div>
                  {community.activityBadge && (
                    <div className="flex items-center gap-1 text-tertiary">
                      <span className="material-symbols-outlined text-[14px]">local_fire_department</span>
                      {community.activityBadge}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}
function TagsExplorerPage() {
  const { request } = useApi();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/tags/summary").then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  if (loading) return <div className="p-8 text-on-surface-variant flex items-center gap-2"><Loader2 className="animate-spin" /> Loading tags...</div>;
  if (!data) return <div className="p-8 text-error">Failed to load tags.</div>;

  return (
    <div className="p-margin-mobile md:p-gutter max-w-container-max mx-auto min-h-screen pb-16">
      <div className="mb-stack-lg border-b border-white/10 pb-stack-md flex flex-col md:flex-row md:items-end justify-between gap-stack-md mt-6">
        <div>
          <h1 className="font-headline-xl text-headline-xl text-on-surface mb-2">Explore Tags</h1>
          <p className="font-body-md text-body-md text-on-surface-variant max-w-2xl">
            Tags act as the central nervous system of DevStack. Follow tags to curate your feed, discover niche communities, and find specialized technical solutions.
          </p>
        </div>
      </div>
      
      <section>
        <div className="flex items-center justify-between border-b border-white/10 pb-stack-sm mb-stack-md">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-outline">sort_by_alpha</span>
            <h2 className="font-headline-md text-headline-md text-on-surface">Tag Directory</h2>
          </div>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {data.items?.length === 0 ? (
            <div className="col-span-full py-12 text-center text-on-surface-variant">No tags found.</div>
          ) : (
            data.items?.map(tag => (
              <div key={tag.id} className="bg-surface-container-low border border-white/5 rounded-xl p-stack-md group hover:border-primary/40 transition-all duration-300 flex flex-col justify-between h-full">
                <div>
                  <div className="flex justify-between items-start mb-stack-sm">
                    <Link to={`/search?q=${encodeURIComponent(tag.name)}&category=articles`} className="inline-flex items-center font-label-sm text-label-sm bg-surface-bright border border-outline-variant text-on-surface px-2 py-1 rounded hover:bg-surface-variant transition-colors">
                      {tag.name}
                    </Link>
                    <span className="material-symbols-outlined text-outline group-hover:text-primary transition-colors">arrow_outward</span>
                  </div>
                  <p className="font-body-sm text-sm text-on-surface-variant mt-2 line-clamp-3">
                    {tag.description || `Discussions and resources related to ${tag.name}.`}
                  </p>
                </div>
                <div className="mt-stack-md flex justify-between items-center font-label-sm text-label-sm">
                  <div className="text-on-surface-variant flex items-center gap-1">
                    <span className="material-symbols-outlined text-[14px]">article</span> {tag.postCount} posts
                  </div>
                  <button className="text-primary hover:bg-primary/10 px-2 py-1 rounded flex items-center gap-1 transition-colors">
                    {tag.isFollowed ? 'Following' : 'Follow'}
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </section>
    </div>
  );
}
function SearchResultsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get("q") || "";
  const category = searchParams.get("category") || "articles";
  const { request } = useApi();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let mounted = true;
    if (!query) {
      setData(null);
      return;
    }
    setLoading(true);
    request(`/api/search?q=${encodeURIComponent(query)}&category=${encodeURIComponent(category)}&page=1&pageSize=20`).then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [query, category, request]);

  return (
    <div className="flex-1 max-w-container-max mx-auto w-full px-gutter py-stack-lg flex flex-col lg:flex-row gap-gutter">
      <div className="flex-1 flex flex-col gap-6 max-w-[720px]">
        <div className="flex flex-col gap-4 border-b border-white/10 pb-4">
          <div>
            <h1 className="font-headline-lg text-headline-lg text-on-surface mb-2 flex items-center gap-3">
              <span className="material-symbols-outlined text-primary" style={{fontSize: "32px"}}>search</span>
              Results for "<span className="text-primary">{query}</span>"
            </h1>
            <p className="font-body-md text-body-md text-on-surface-variant">
              {data ? `Found ${data.totalCount} results` : "Enter a query to search"}
            </p>
          </div>
          <div className="flex gap-1 overflow-x-auto pb-1">
            {["articles", "tags", "authors"].map(cat => (
              <button 
                key={cat}
                onClick={() => setSearchParams({ q: query, category: cat })}
                className={`px-4 py-2 font-label-md text-label-md rounded capitalize transition-colors ${category === cat ? "text-primary border-b-2 border-primary bg-primary/5 rounded-t" : "text-on-surface-variant hover:text-on-surface hover:bg-surface-container/50"}`}
              >
                {cat}
              </button>
            ))}
          </div>
        </div>
        
        <div className="flex flex-col gap-4">
          {loading ? (
             <div className="p-8 text-on-surface-variant flex items-center gap-2"><Loader2 className="animate-spin" /> Searching...</div>
          ) : !data || data.items?.length === 0 ? (
            <div className="p-8 text-center bg-surface-container/20 rounded-lg border border-white/5">
              <span className="material-symbols-outlined text-4xl text-on-surface-variant mb-2">search_off</span>
              <h3 className="font-headline-md text-on-surface">No results found</h3>
              <p className="text-on-surface-variant">We couldn't find any {category} matching "{query}".</p>
            </div>
          ) : (
            data.items.map((item, i) => (
              <article key={i} className="bg-surface-container/40 backdrop-blur-md border border-white/5 rounded-lg p-5 flex flex-col gap-3 hover:border-primary/30 transition-colors group cursor-pointer" onClick={() => category === 'articles' && item.slug && (window.location.href = `/articles/${item.slug}`)}>
                {category === 'articles' && (
                  <>
                    <h2 className="font-headline-md text-headline-md text-on-surface leading-tight group-hover:text-primary transition-colors">{item.title}</h2>
                    <div className="flex gap-1 mb-2">
                      {item.tags?.map(t => <span key={t} className="font-label-sm px-2 py-0.5 bg-surface-container rounded text-on-surface-variant">{t}</span>)}
                    </div>
                  </>
                )}
                {category === 'tags' && (
                  <>
                    <h2 className="font-headline-md text-headline-md text-on-surface leading-tight group-hover:text-primary transition-colors">#{item.name}</h2>
                    <p className="text-on-surface-variant">{item.postCount} posts</p>
                  </>
                )}
                {category === 'authors' && (
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-surface-bright flex items-center justify-center font-bold">{item.displayName?.[0] || item.username?.[0] || '?'}</div>
                    <div>
                      <h2 className="font-headline-md text-lg text-on-surface leading-tight group-hover:text-primary transition-colors">{item.displayName || item.username}</h2>
                      <p className="text-on-surface-variant text-sm">@{item.username}</p>
                    </div>
                  </div>
                )}
              </article>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
function NotificationsPage() {
  const { request } = useApi();
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/notifications").then(data => {
      if (mounted) { setNotifications(data.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const markAsRead = async (id) => {
    try {
      await request(`/api/notifications/${id}/read`, { method: "PUT" });
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    } catch {
      // Leave the notification unread if the server rejects the update.
    }
  };

  const markAllAsRead = async () => {
    try {
      await request("/api/notifications/read-all", { method: "POST" });
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch {
      // Leave local read states unchanged if the bulk update fails.
    }
  };

  return (
    <div className="flex justify-center p-gutter w-full flex-1">
      <div className="w-full max-w-[720px] flex flex-col gap-stack-lg py-stack-lg">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <h1 className="font-headline-lg-mobile md:font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface">Notifications</h1>
          <button onClick={markAllAsRead} className="text-primary hover:text-primary-fixed transition-colors font-label-sm text-label-sm flex items-center gap-1 w-fit">
            <span className="material-symbols-outlined text-[16px]">done_all</span>
            Mark all as read
          </button>
        </div>

        {/* Achievements Highlight Bento */}
        <div className="bg-surface-container/60 backdrop-blur-md rounded-xl p-6 relative overflow-hidden flex flex-col sm:flex-row items-start sm:items-center gap-6 border border-white/5">
          <div className="absolute -right-20 -top-20 w-64 h-64 bg-secondary-container rounded-full blur-[80px] opacity-20 pointer-events-none"></div>
          <div className="w-16 h-16 rounded-full bg-gradient-to-br from-secondary-container to-tertiary p-[1px] shrink-0">
            <div className="w-full h-full bg-surface-container rounded-full flex items-center justify-center">
              <span className="material-symbols-outlined text-tertiary text-3xl">military_tech</span>
            </div>
          </div>
          <div className="flex-1 z-10">
            <div className="flex items-center gap-2 mb-1">
              <span className="font-label-sm text-label-sm text-tertiary tracking-wider uppercase">New Achievement</span>
              <span className="w-1.5 h-1.5 rounded-full bg-primary"></span>
            </div>
            <h3 className="font-headline-md text-headline-md text-on-surface mb-2">Code Whisperer II</h3>
            <p className="font-body-md text-body-md text-on-surface-variant">Your reputation in <span className="text-primary cursor-pointer hover:underline">#rust-lang</span> has reached top 5%. You've unlocked new moderation privileges.</p>
          </div>
          <button className="shrink-0 bg-surface-bright hover:bg-surface-container-highest text-on-surface px-4 py-2 rounded-lg font-label-md text-label-md border border-outline-variant transition-colors mt-4 sm:mt-0 z-10">
            View Perks
          </button>
        </div>

        {/* Filtering Tabs */}
        <div className="flex gap-2 border-b border-surface-variant pb-2 overflow-x-auto no-scrollbar">
          <button className="px-4 py-1.5 rounded-full bg-primary-container/20 text-primary font-label-md text-label-md whitespace-nowrap border border-primary/30">All</button>
          <button className="px-4 py-1.5 rounded-full text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors font-label-md text-label-md whitespace-nowrap">Mentions</button>
          <button className="px-4 py-1.5 rounded-full text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors font-label-md text-label-md whitespace-nowrap flex items-center gap-2">
            Replies <span className="bg-primary/20 text-primary px-1.5 py-0.5 rounded text-[10px] leading-none">2</span>
          </button>
          <button className="px-4 py-1.5 rounded-full text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors font-label-md text-label-md whitespace-nowrap">Likes</button>
        </div>

        {/* Feed Container */}
        <div className="flex flex-col gap-stack-sm">
          {loading ? (
            <div className="flex items-center gap-2 text-on-surface-variant"><Loader2 className="animate-spin" /> Loading notifications...</div>
          ) : notifications.length === 0 ? (
            <div className="p-8 text-center text-on-surface-variant bg-surface-container/20 rounded-lg border border-white/5">You're all caught up.</div>
          ) : (
            notifications.map(n => (
              <div key={n.id} onClick={() => markAsRead(n.id)} className={`bg-surface-container/60 backdrop-blur-md rounded-xl p-stack-md flex gap-4 cursor-pointer hover:bg-surface-container-high/50 transition-colors relative group border border-white/5 ${n.isRead ? 'opacity-75 hover:opacity-100' : ''}`}>
                {!n.isRead && <div className="absolute left-0 top-3 bottom-3 w-1 bg-primary rounded-r"></div>}
                
                <div className="shrink-0 pt-1 relative">
                  {n.type === 'mention' || n.type === 'reply' ? (
                    n.actorAvatar ? (
                      <img src={n.actorAvatar} alt={n.actorName} className="w-10 h-10 rounded-full border border-outline-variant object-cover" />
                    ) : (
                      <div className="w-10 h-10 rounded-full bg-secondary-container/30 border border-secondary-container flex items-center justify-center text-secondary">
                        <span className="material-symbols-outlined text-[20px]">{n.type === 'reply' ? 'forum' : 'alternate_email'}</span>
                      </div>
                    )
                  ) : n.type === 'like' ? (
                    <div className="w-10 h-10 rounded-full bg-error-container/20 border border-error-container/50 flex items-center justify-center text-error">
                      <span className="material-symbols-outlined text-[20px]">favorite</span>
                    </div>
                  ) : (
                    <div className="w-10 h-10 rounded-full bg-surface-variant border border-outline-variant flex items-center justify-center text-on-surface-variant">
                      <span className="material-symbols-outlined text-[20px]">merge</span>
                    </div>
                  )}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="flex justify-between items-start gap-4 mb-1">
                    <p className="font-body-md text-body-md text-on-surface leading-tight">
                      <span className="font-bold">{n.actorName}</span> {n.type === 'mention' ? 'mentioned you in' : n.type === 'reply' ? 'replied to your comment in' : n.type === 'like' ? 'liked your solution on' : n.type} <span className="font-label-md text-label-md text-primary">{n.targetTitle}</span>
                    </p>
                    <span className="font-label-sm text-label-sm text-on-surface-variant shrink-0">{new Date(n.createdAt).toLocaleDateString()}</span>
                  </div>
                  {n.contentExcerpt && (
                    <div className="bg-surface-dim rounded-lg p-3 border border-surface-variant mt-2">
                      <p className="font-body-md text-body-md text-on-surface-variant line-clamp-2">{n.contentExcerpt}</p>
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
function CommunitiesPage() {
  const { request } = useApi();
  const [communities, setCommunities] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/communities/followed").then(data => {
      if (mounted) { setCommunities(data.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const toggleFollow = async (id, isFollowed) => {
    const method = isFollowed ? "DELETE" : "POST";
    try {
      await request(`/api/communities/${id}/follow`, { method });
      setCommunities(prev => prev.filter(c => c.id !== id));
    } catch {
      // Keep the community visible if the follow action fails.
    }
  };

  return (
    <div className="flex-1 max-w-container-max mx-auto w-full px-gutter py-stack-lg">
      <header className="mb-stack-lg flex flex-col md:flex-row md:items-end justify-between gap-4 border-b border-white/5 pb-stack-md">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface font-bold">Communities</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-2">Hubs you follow and participate in.</p>
        </div>
        <Link to="/communities/discover" className="bg-primary text-on-primary px-4 py-2 rounded-lg font-label-md text-label-md hover:bg-primary-fixed transition-colors flex items-center gap-2 w-fit">
          <span className="material-symbols-outlined text-[18px]">explore</span>
          Discover More
        </Link>
      </header>

      {loading ? (
        <div className="flex items-center gap-2 text-on-surface-variant"><Loader2 className="animate-spin" /> Loading communities...</div>
      ) : communities.length === 0 ? (
        <div className="p-8 text-center bg-surface-container/20 rounded-lg border border-white/5">
          <p className="text-on-surface-variant">You haven't joined any communities yet.</p>
          <Link to="/communities/discover" className="mt-4 inline-block text-primary hover:underline">Explore communities</Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-stack-md">
          {communities.map(c => (
            <article key={c.id} className="bg-surface-container border border-white/5 rounded-xl p-5 hover:border-white/10 transition-colors flex flex-col group relative overflow-hidden">
              <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"></div>
              <div className="flex justify-between items-start mb-4 relative z-10">
                <div className="w-12 h-12 rounded bg-surface-dim border border-white/10 flex items-center justify-center text-primary text-xl font-bold uppercase">
                  {c.name[0]}
                </div>
                <button onClick={() => toggleFollow(c.id, c.isFollowed)} className="font-label-sm text-label-sm px-3 py-1 rounded border border-white/10 text-on-surface-variant hover:border-primary hover:text-primary transition-colors bg-surface-container-high/50 z-20 relative">
                  Joined
                </button>
              </div>
              <h3 className="font-headline-md text-headline-md text-on-surface mb-2 relative z-10 font-bold">{c.name}</h3>
              <p className="font-body-md text-body-md text-on-surface-variant flex-1 mb-4 relative z-10 line-clamp-2">{c.description}</p>
              <div className="flex items-center gap-4 mt-auto pt-4 border-t border-white/5 relative z-10">
                <div className="flex items-center gap-1.5 font-label-sm text-label-sm text-on-surface-variant">
                  <span className="material-symbols-outlined text-[16px]">group</span>
                  {c.memberCount} Members
                </div>
                {c.activityBadge && (
                  <div className="flex items-center gap-1.5 font-label-sm text-label-sm text-tertiary">
                    <span className="w-2 h-2 rounded-full bg-tertiary animate-pulse"></span>
                    {c.activityBadge}
                  </div>
                )}
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}

function DiscoverCommunitiesPage() {
  const { request } = useApi();
  const [communities, setCommunities] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/communities/recommended").then(data => {
      if (mounted) { setCommunities(data.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const toggleFollow = async (id, isFollowed) => {
    const method = isFollowed ? "DELETE" : "POST";
    try {
      await request(`/api/communities/${id}/follow`, { method });
      setCommunities(prev => prev.map(c => c.id === id ? { ...c, isFollowed: !isFollowed } : c));
    } catch {
      // Preserve the current follow state if the request fails.
    }
  };

  return (
    <div className="flex-1 max-w-container-max mx-auto w-full px-gutter py-stack-lg">
      <header className="mb-stack-lg flex flex-col md:flex-row md:items-end justify-between gap-4 border-b border-white/5 pb-stack-md">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface font-bold">Discover Communities</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-2">Find new communities matching your interests.</p>
        </div>
        <Link to="/communities" className="bg-surface-bright text-on-surface px-4 py-2 rounded-lg font-label-md text-label-md hover:bg-surface-variant transition-colors flex items-center gap-2 w-fit border border-white/10">
          <span className="material-symbols-outlined text-[18px]">arrow_back</span>
          Back to My Communities
        </Link>
      </header>

      {loading ? (
        <div className="flex items-center gap-2 text-on-surface-variant"><Loader2 className="animate-spin" /> Loading communities...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-stack-md">
          {communities.map(c => (
            <article key={c.id} className="bg-surface-container border border-white/5 rounded-xl p-5 hover:border-white/10 transition-colors flex flex-col group relative overflow-hidden">
              <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"></div>
              <div className="flex justify-between items-start mb-4 relative z-10">
                <div className="w-12 h-12 rounded bg-surface-dim border border-white/10 flex items-center justify-center text-primary text-xl font-bold uppercase">
                  {c.name[0]}
                </div>
                <button onClick={() => toggleFollow(c.id, c.isFollowed)} className={`font-label-sm text-label-sm px-3 py-1 rounded transition-colors z-20 relative ${c.isFollowed ? 'border border-white/10 text-on-surface-variant hover:border-primary hover:text-primary bg-surface-container-high/50' : 'bg-primary text-on-primary hover:bg-primary-fixed'}`}>
                  {c.isFollowed ? 'Joined' : 'Join'}
                </button>
              </div>
              <h3 className="font-headline-md text-headline-md text-on-surface mb-2 relative z-10 font-bold">{c.name}</h3>
              <p className="font-body-md text-body-md text-on-surface-variant flex-1 mb-4 relative z-10 line-clamp-2">{c.description}</p>
              <div className="flex items-center gap-4 mt-auto pt-4 border-t border-white/5 relative z-10">
                <div className="flex items-center gap-1.5 font-label-sm text-label-sm text-on-surface-variant">
                  <span className="material-symbols-outlined text-[16px]">group</span>
                  {c.memberCount} Members
                </div>
                {c.activityBadge && (
                  <div className="flex items-center gap-1.5 font-label-sm text-label-sm text-tertiary">
                    <span className="w-2 h-2 rounded-full bg-tertiary animate-pulse"></span>
                    {c.activityBadge}
                  </div>
                )}
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}

function CustomFeedsPage() {
  const { request } = useApi();
  const [feeds, setFeeds] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request("/api/feeds/custom").then(data => {
      if (mounted) { setFeeds(data.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  return (
    <div className="flex-1 flex flex-col md:flex-row gap-gutter p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full">
      <aside className="w-full md:w-80 flex flex-col gap-stack-md border-r border-white/5 pr-gutter">
        <div className="flex justify-between items-center">
          <h1 className="font-headline-md text-headline-md text-on-surface">My Feeds</h1>
          <button className="w-8 h-8 rounded-full bg-surface-container-high flex items-center justify-center text-primary hover:bg-surface-bright transition-colors border border-white/10" title="Create New Feed">
            <span className="material-symbols-outlined text-sm">add</span>
          </button>
        </div>
        <div className="flex-1 overflow-y-auto pr-2 space-y-2">
          {loading ? <div className="text-on-surface-variant"><Loader2 className="animate-spin" /></div> : feeds.map((f, i) => (
            <div key={f.id} className={`p-3 rounded-xl cursor-pointer transition-colors ${i === 0 ? 'bg-surface-container border-l-2 border-primary hover:bg-surface-container-high' : 'bg-surface border border-transparent hover:border-white/5 hover:bg-surface-container-low'}`}>
              <div className="flex justify-between items-start mb-1">
                <h3 className={`font-body-md ${i === 0 ? 'font-semibold text-primary' : 'text-on-surface'}`}>{f.name}</h3>
                <span className="material-symbols-outlined text-outline text-sm">more_horiz</span>
              </div>
              <p className="font-label-sm text-label-sm text-on-surface-variant line-clamp-1 mb-2">tags: {f.tags?.join(', ')}</p>
            </div>
          ))}
        </div>
      </aside>
      <section className="flex-1 flex flex-col gap-stack-lg overflow-y-auto pb-gutter">
        {!loading && feeds[0] && (
          <>
            <div className="bg-surface-container/50 border border-white/5 rounded-2xl p-6 backdrop-blur-md">
              <div className="flex justify-between items-start">
                <div>
                  <div className="flex items-center gap-3 mb-2">
                    <span className="material-symbols-outlined text-primary text-2xl">dynamic_feed</span>
                    <h2 className="font-headline-lg text-headline-lg text-on-surface">{feeds[0].name}</h2>
                  </div>
                  <p className="font-body-md text-body-md text-on-surface-variant max-w-2xl">A curated stream combining selected communities and tags.</p>
                </div>
                <button className="bg-surface-bright hover:bg-surface-variant text-on-surface px-4 py-2 rounded-lg font-label-md border border-white/10 transition-colors flex items-center gap-2">
                  <span className="material-symbols-outlined text-sm">edit</span> Edit Rules
                </button>
              </div>
              <div className="mt-4 flex gap-2">
                {feeds[0].tags?.map(t => <span key={t} className="px-3 py-1 rounded bg-surface border border-outline-variant font-label-sm text-tertiary">#{t}</span>)}
                <span className="px-3 py-1 rounded bg-surface border border-outline-variant font-label-sm text-on-surface-variant">sort: {feeds[0].sortOrder}</span>
              </div>
            </div>
            <div className="p-8 text-center bg-surface-container/20 rounded-lg border border-white/5">
              <p className="text-on-surface-variant">Feed items will appear here based on your rules.</p>
            </div>
          </>
        )}
      </section>
    </div>
  );
}

function ProfileSettingsPage() {
  const api = useApi();
  const { request } = api;
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState("");
  const [error, setError] = useState("");
  const { uploading } = usePresignedUpload(api);

  useEffect(() => {
    let mounted = true;
    request("/api/users/profile-settings").then(data => {
      if (mounted) { setProfile(data); setLoading(false); }
    }).catch(err => {
      if (mounted) { setError(err.message); setLoading(false); }
    });
    return () => { mounted = false; };
  }, [request]);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError("");
    setSuccess("");
    try {
      await request("/api/users/profile-settings", {
        method: "PUT",
        body: JSON.stringify({
          displayName: profile.displayName,
          bio: profile.bio,
          avatarStorageKey: profile.avatarUrl
        })
      });
      setSuccess("Profile updated successfully.");
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="p-8"><Loader2 className="animate-spin" /></div>;
  if (!profile) return <div className="p-8 text-error">Failed to load profile.</div>;

  return (
    <div className="flex-1 p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full">
      <header className="mb-stack-lg">
        <h1 className="font-headline-xl text-headline-xl text-on-surface mb-2">Settings</h1>
        <p className="font-body-lg text-body-lg text-on-surface-variant">Manage your account settings and preferences.</p>
      </header>
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-stack-lg">
        <div className="lg:col-span-3 lg:block hidden">
          <nav className="sticky top-24 flex flex-col gap-1">
            <Link to="/settings/profile" className="px-4 py-2 rounded-lg bg-surface-container-highest text-primary font-bold transition-colors">Profile</Link>
            <Link to="/settings/account" className="px-4 py-2 rounded-lg text-on-surface-variant hover:bg-surface-container-highest hover:text-on-surface transition-colors">Account</Link>
          </nav>
        </div>
        <div className="lg:col-span-9 flex flex-col gap-stack-lg">
          <section className="bg-surface-container/60 backdrop-blur-md border border-white/5 rounded-xl p-stack-md md:p-stack-lg" id="profile">
            <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-stack-md border-b border-white/5 pb-4">Profile</h2>
            {error && <div className="mb-4 p-3 bg-error-container/20 border border-error-container text-error rounded-md">{error}</div>}
            {success && <div className="mb-4 p-3 bg-primary/20 border border-primary text-primary rounded-md">{success}</div>}
            
            <form onSubmit={handleSave}>
              <div className="mb-stack-md">
                <label className="block font-label-md text-label-md text-on-surface-variant mb-2">Cover Image</label>
                <div className="h-32 rounded-lg bg-surface-container border border-outline-variant relative overflow-hidden group cursor-pointer">
                  <div className="absolute inset-0 bg-gradient-to-r from-primary/20 to-tertiary/20"></div>
                  <div className="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity">
                    <span className="material-symbols-outlined text-white">photo_camera</span>
                  </div>
                </div>
              </div>
              <div className="flex flex-col sm:flex-row gap-stack-md items-start sm:items-center mb-stack-lg">
                <div className="relative">
                  <div className="w-24 h-24 rounded-full object-cover border-4 border-surface bg-surface-bright flex items-center justify-center font-headline-xl text-on-surface">
                    {profile.avatarUrl ? <img src={profile.avatarUrl} className="w-full h-full rounded-full" alt="" /> : (profile.displayName?.[0] || profile.username?.[0] || 'U').toUpperCase()}
                  </div>
                  <button type="button" className="absolute bottom-0 right-0 bg-surface-variant rounded-full p-1.5 border border-outline-variant hover:bg-surface-bright transition-colors">
                    <span className="material-symbols-outlined text-sm">edit</span>
                  </button>
                </div>
                <div className="flex-1 space-y-4 w-full">
                  <div>
                    <label className="block font-label-md text-label-md text-on-surface-variant mb-1">Display Name</label>
                    <input className="w-full bg-surface-container border border-outline-variant rounded-lg py-2 px-3 text-on-surface focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all font-body-md" type="text" value={profile.displayName || ""} onChange={e => setProfile({...profile, displayName: e.target.value})} />
                  </div>
                </div>
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block font-label-md text-label-md text-on-surface-variant mb-1">Bio</label>
                  <textarea className="w-full bg-surface-container border border-outline-variant rounded-lg py-2 px-3 text-on-surface focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all font-body-md resize-none" rows="3" value={profile.bio || ""} onChange={e => setProfile({...profile, bio: e.target.value})}></textarea>
                </div>
              </div>
              <div className="mt-stack-md flex justify-end">
                <button type="submit" disabled={saving || uploading} className="px-6 py-2 bg-primary text-on-primary rounded-lg font-bold hover:bg-primary/90 transition-colors disabled:opacity-50">
                  {saving ? "Saving..." : "Save Changes"}
                </button>
              </div>
            </form>
          </section>
        </div>
      </div>
    </div>
  );
}

function AccountSettingsPage() {
  const { request } = useApi();
  const [passwords, setPasswords] = useState({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState("");
  const [error, setError] = useState("");

  const handleSave = async (e) => {
    e.preventDefault();
    if (passwords.newPassword !== passwords.confirmNewPassword) {
      setError("Passwords do not match.");
      return;
    }
    setSaving(true);
    setError("");
    setSuccess("");
    try {
      await request("/api/users/security/password", {
        method: "PUT",
        body: JSON.stringify(passwords)
      });
      setSuccess("Password updated successfully.");
      setPasswords({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="flex-1 p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full">
      <header className="mb-stack-lg">
        <h1 className="font-headline-xl text-headline-xl text-on-surface mb-2">Settings</h1>
        <p className="font-body-lg text-body-lg text-on-surface-variant">Manage your account settings and preferences.</p>
      </header>
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-stack-lg">
        <div className="lg:col-span-3 lg:block hidden">
          <nav className="sticky top-24 flex flex-col gap-1">
            <Link to="/settings/profile" className="px-4 py-2 rounded-lg text-on-surface-variant hover:bg-surface-container-highest hover:text-on-surface transition-colors">Profile</Link>
            <Link to="/settings/account" className="px-4 py-2 rounded-lg bg-surface-container-highest text-primary font-bold transition-colors">Account</Link>
          </nav>
        </div>
        <div className="lg:col-span-9 flex flex-col gap-stack-lg">
          <section className="bg-surface-container/60 backdrop-blur-md border border-white/5 rounded-xl p-stack-md md:p-stack-lg" id="account">
            <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-stack-md border-b border-white/5 pb-4">Account Security</h2>
            {error && <div className="mb-4 p-3 bg-error-container/20 border border-error-container text-error rounded-md">{error}</div>}
            {success && <div className="mb-4 p-3 bg-primary/20 border border-primary text-primary rounded-md">{success}</div>}
            
            <form onSubmit={handleSave} className="space-y-4">
              <div>
                <label className="block font-label-md text-label-md text-on-surface-variant mb-1">Current Password</label>
                <input required className="w-full bg-surface-container border border-outline-variant rounded-lg py-2 px-3 text-on-surface focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all font-body-md" type="password" value={passwords.currentPassword} onChange={e => setPasswords({...passwords, currentPassword: e.target.value})} />
              </div>
              <div>
                <label className="block font-label-md text-label-md text-on-surface-variant mb-1">New Password</label>
                <input required className="w-full bg-surface-container border border-outline-variant rounded-lg py-2 px-3 text-on-surface focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all font-body-md" type="password" value={passwords.newPassword} onChange={e => setPasswords({...passwords, newPassword: e.target.value})} />
              </div>
              <div>
                <label className="block font-label-md text-label-md text-on-surface-variant mb-1">Confirm New Password</label>
                <input required className="w-full bg-surface-container border border-outline-variant rounded-lg py-2 px-3 text-on-surface focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all font-body-md" type="password" value={passwords.confirmNewPassword} onChange={e => setPasswords({...passwords, confirmNewPassword: e.target.value})} />
              </div>
              <div className="mt-stack-md flex justify-end">
                <button type="submit" disabled={saving} className="px-6 py-2 bg-primary text-on-primary rounded-lg font-bold hover:bg-primary/90 transition-colors disabled:opacity-50">
                  {saving ? "Updating..." : "Update Password"}
                </button>
              </div>
            </form>
          </section>
        </div>
      </div>
    </div>
  );
}
function UserAnalyticsPage() {
  const { username } = useParams();
  const { request } = useApi();
  const [period, setPeriod] = useState('30d');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    request(`/api/users/${username || 'me'}/analytics/summary?period=${period}`).then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request, username, period]);

  if (loading || !data) return <div className="p-8 text-on-surface-variant flex justify-center mt-10"><Loader2 className="animate-spin mr-2"/> Loading analytics...</div>;

  const maxViews = Math.max(...data.dailyViews.map(d => d.count), 10);
  
  return (
    <div className="flex-1 p-margin-mobile md:p-gutter min-h-screen pt-16">
      <div className="max-w-[1000px] mx-auto">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-stack-lg gap-4">
          <div>
            <h1 className="font-headline-xl text-headline-lg-mobile md:text-headline-xl text-on-surface mb-2">User Analytics</h1>
            <p className="font-body-lg text-body-lg text-on-surface-variant">Engagement overview for {data.username}</p>
          </div>
          <div className="flex bg-surface-container rounded-lg p-1 border border-outline-variant">
            <button onClick={() => setPeriod('7d')} className={`px-4 py-1.5 rounded-md font-label-md text-label-md transition-colors ${period === '7d' ? 'bg-surface-bright text-on-surface shadow-sm' : 'text-on-surface-variant hover:text-on-surface'}`}>7 Days</button>
            <button onClick={() => setPeriod('30d')} className={`px-4 py-1.5 rounded-md font-label-md text-label-md transition-colors ${period === '30d' ? 'bg-surface-bright text-on-surface shadow-sm' : 'text-on-surface-variant hover:text-on-surface'}`}>30 Days</button>
          </div>
        </div>

        {/* Metrics Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-stack-lg">
          <div className="bg-surface-container/50 border border-white/5 backdrop-blur-md rounded-xl p-stack-md flex flex-col justify-between hover:bg-surface-container/80 transition-colors">
            <div className="flex justify-between items-start mb-4">
              <span className="font-label-md text-label-md text-on-surface-variant">Total Views</span>
              <div className="w-8 h-8 rounded bg-primary/10 flex items-center justify-center text-primary">
                <span className="material-symbols-outlined text-[18px]">visibility</span>
              </div>
            </div>
            <span className="font-headline-lg text-headline-lg text-on-surface">{data.totalViews.toLocaleString()}</span>
          </div>
          <div className="bg-surface-container/50 border border-white/5 backdrop-blur-md rounded-xl p-stack-md flex flex-col justify-between hover:bg-surface-container/80 transition-colors">
            <div className="flex justify-between items-start mb-4">
              <span className="font-label-md text-label-md text-on-surface-variant">Votes Received</span>
              <div className="w-8 h-8 rounded bg-secondary/10 flex items-center justify-center text-secondary">
                <span className="material-symbols-outlined text-[18px]">thumb_up</span>
              </div>
            </div>
            <span className="font-headline-lg text-headline-lg text-on-surface">{data.votesReceived.toLocaleString()}</span>
          </div>
          <div className="bg-surface-container/50 border border-white/5 backdrop-blur-md rounded-xl p-stack-md flex flex-col justify-between hover:bg-surface-container/80 transition-colors">
            <div className="flex justify-between items-start mb-4">
              <span className="font-label-md text-label-md text-on-surface-variant">Comments</span>
              <div className="w-8 h-8 rounded bg-tertiary/10 flex items-center justify-center text-tertiary">
                <span className="material-symbols-outlined text-[18px]">forum</span>
              </div>
            </div>
            <span className="font-headline-lg text-headline-lg text-on-surface">{data.commentCount.toLocaleString()}</span>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-stack-lg">
          {/* Custom SVG Line Chart */}
          <div className="lg:col-span-2 bg-surface-container/50 border border-white/5 rounded-xl p-6">
            <h3 className="font-headline-md text-headline-md text-on-surface mb-6">Daily Views</h3>
            <div className="relative h-64 border-l border-b border-surface-bright pb-2 pl-2">
              <svg className="w-full h-full overflow-visible" preserveAspectRatio="none">
                {data.dailyViews.map((d, i) => {
                  const x = (i / (data.dailyViews.length - 1)) * 100 + "%";
                  const y = 100 - (d.count / maxViews) * 100 + "%";
                  if (i === 0) return null;
                  const prev = data.dailyViews[i - 1];
                  const prevX = ((i - 1) / (data.dailyViews.length - 1)) * 100 + "%";
                  const prevY = 100 - (prev.count / maxViews) * 100 + "%";
                  return (
                    <line key={i} x1={prevX} y1={prevY} x2={x} y2={y} stroke="#adc6ff" strokeWidth="2" />
                  );
                })}
                {data.dailyViews.map((d, i) => {
                  const x = (i / (data.dailyViews.length - 1)) * 100 + "%";
                  const y = 100 - (d.count / maxViews) * 100 + "%";
                  return (
                    <circle key={`c-${i}`} cx={x} cy={y} r="4" fill="#adc6ff" className="hover:r-6 transition-all cursor-pointer">
                      <title>{d.date}: {d.count} views</title>
                    </circle>
                  );
                })}
              </svg>
            </div>
          </div>

          {/* Top Tags Grid */}
          <div className="bg-surface-container/50 border border-white/5 rounded-xl p-6">
            <h3 className="font-headline-md text-headline-md text-on-surface mb-6">Top Tags</h3>
            <div className="flex flex-col gap-4">
              {data.topTags.map((tag, i) => (
                <div key={i} className="flex justify-between items-center group cursor-pointer">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded bg-surface-container-highest flex items-center justify-center text-primary font-label-sm">#</div>
                    <div>
                      <div className="font-label-md text-label-md text-on-surface group-hover:text-primary transition-colors">{tag.name}</div>
                      <div className="font-label-sm text-label-sm text-on-surface-variant">{tag.postCount} posts</div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}

function DetailedAnalyticsPage() {
  const { request } = useApi();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request('/api/analytics/community-detailed').then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const exportCsv = () => alert("Exporting CSV...");

  if (loading || !data) return <div className="p-8 text-on-surface-variant flex justify-center mt-10"><Loader2 className="animate-spin mr-2"/> Loading detailed analytics...</div>;

  return (
    <div className="flex-1 p-margin-mobile md:p-gutter min-h-screen pt-16">
      <div className="max-w-container-max mx-auto">
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-stack-lg gap-4">
          <div>
            <h1 className="font-headline-xl text-headline-lg-mobile md:text-headline-xl text-on-surface mb-2">Platform Analytics</h1>
            <p className="font-body-lg text-body-lg text-on-surface-variant">Real-time metrics for DevStack engagement and growth.</p>
          </div>
          <div className="flex gap-2">
            <select className="bg-surface-container border border-outline-variant rounded px-3 py-2 font-label-md text-label-md text-on-surface focus:border-primary focus:ring-1 focus:ring-primary appearance-none pr-8 relative">
              <option>Last 24 Hours</option>
              <option defaultValue="Last 7 Days">Last 7 Days</option>
              <option>Last 30 Days</option>
              <option>Year to Date</option>
            </select>
            <button onClick={exportCsv} className="px-4 py-2 bg-surface-container border border-outline-variant rounded hover:bg-surface-bright transition-colors font-label-md text-label-md flex items-center gap-2 text-on-surface">
              <span className="material-symbols-outlined text-[18px]">download</span>
              Export
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-stack-lg">
          <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-4 flex flex-col justify-between relative overflow-hidden group">
            <div className="absolute -right-4 -top-4 w-24 h-24 bg-primary/10 rounded-full blur-xl group-hover:bg-primary/20 transition-all duration-500"></div>
            <div className="flex justify-between items-start mb-4 relative z-10">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Total Active Users</span>
              <span className="material-symbols-outlined text-primary">group</span>
            </div>
            <div className="relative z-10">
              <div className="font-headline-lg text-headline-lg text-on-surface mb-1">{data.activeUserCount?.toLocaleString() || 0}</div>
              <div className="flex items-center gap-1 font-label-sm text-label-sm text-tertiary">
                <span className="material-symbols-outlined text-[14px]">trending_up</span>
                <span>+12.5% vs last period</span>
              </div>
            </div>
          </div>
          
          <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-4 flex flex-col justify-between relative overflow-hidden group">
            <div className="absolute -right-4 -top-4 w-24 h-24 bg-secondary/10 rounded-full blur-xl group-hover:bg-secondary/20 transition-all duration-500"></div>
            <div className="flex justify-between items-start mb-4 relative z-10">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Total Posts</span>
              <span className="material-symbols-outlined text-secondary">forum</span>
            </div>
            <div className="relative z-10">
              <div className="font-headline-lg text-headline-lg text-on-surface mb-1">{data.totalPostCount?.toLocaleString() || 0}</div>
              <div className="flex items-center gap-1 font-label-sm text-label-sm text-tertiary">
                <span className="material-symbols-outlined text-[14px]">trending_up</span>
                <span>+4.2% vs last period</span>
              </div>
            </div>
          </div>

          <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-4 flex flex-col justify-between relative overflow-hidden group">
            <div className="absolute -right-4 -top-4 w-24 h-24 bg-error/10 rounded-full blur-xl group-hover:bg-error/20 transition-all duration-500"></div>
            <div className="flex justify-between items-start mb-4 relative z-10">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Flagged Content</span>
              <span className="material-symbols-outlined text-error">flag</span>
            </div>
            <div className="relative z-10">
              <div className="font-headline-lg text-headline-lg text-on-surface mb-1">{data.flaggedContentCount?.toLocaleString() || 0}</div>
              <div className="flex items-center gap-1 font-label-sm text-label-sm text-error">
                <span className="material-symbols-outlined text-[14px]">trending_down</span>
                <span>-1.5% vs last period</span>
              </div>
            </div>
          </div>

          <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-4 flex flex-col justify-between relative overflow-hidden group">
            <div className="absolute -right-4 -top-4 w-24 h-24 bg-tertiary/10 rounded-full blur-xl group-hover:bg-tertiary/20 transition-all duration-500"></div>
            <div className="flex justify-between items-start mb-4 relative z-10">
              <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">New Users</span>
              <span className="material-symbols-outlined text-tertiary">person_add</span>
            </div>
            <div className="relative z-10">
              <div className="font-headline-lg text-headline-lg text-on-surface mb-1">{data.newUserCount?.toLocaleString() || 0}</div>
              <div className="flex items-center gap-1 font-label-sm text-label-sm text-tertiary">
                <span className="material-symbols-outlined text-[14px]">trending_up</span>
                <span>+18.9% vs last period</span>
              </div>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-stack-lg">
          <div className="lg:col-span-2 bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-6 flex flex-col">
            <div className="flex justify-between items-center mb-6">
              <div>
                <h3 className="font-headline-md text-headline-md text-on-surface">Activity Overview</h3>
                <p className="font-label-sm text-label-sm text-on-surface-variant">Posts and comments over time</p>
              </div>
              <div className="flex gap-2">
                <div className="flex items-center gap-1">
                  <div className="w-3 h-3 rounded-full bg-primary"></div>
                  <span className="font-label-sm text-label-sm text-on-surface-variant">Posts</span>
                </div>
                <div className="flex items-center gap-1">
                  <div className="w-3 h-3 rounded-full bg-secondary"></div>
                  <span className="font-label-sm text-label-sm text-on-surface-variant">Comments</span>
                </div>
              </div>
            </div>
            <div className="flex-1 relative min-h-[300px] border-l border-b border-surface-bright flex items-end pt-4 pb-2 pr-2">
              <div className="absolute left-[-30px] top-0 h-full flex flex-col justify-between text-[10px] font-label-sm text-on-surface-variant pb-2">
                <span>10k</span><span>7.5k</span><span>5k</span><span>2.5k</span><span>0</span>
              </div>
              <div className="absolute inset-0 flex flex-col justify-between pointer-events-none pb-2 pl-2">
                <div className="w-full border-t border-surface-bright/50"></div>
                <div className="w-full border-t border-surface-bright/50"></div>
                <div className="w-full border-t border-surface-bright/50"></div>
                <div className="w-full border-t border-surface-bright/50"></div>
                <div className="w-full"></div>
              </div>
              <div className="w-full h-full flex justify-between items-end gap-1 sm:gap-2 px-2 relative z-10">
                {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map((day, i) => {
                  const h1 = [40, 45, 60, 55, 75, 30, 25][i];
                  const h2 = [20, 25, 35, 30, 45, 15, 10][i];
                  return (
                    <div key={day} className="flex-1 flex flex-col justify-end gap-1 group relative h-full">
                      <div className="w-full bg-secondary/80 rounded-t-sm group-hover:bg-secondary transition-colors absolute bottom-0" style={{height: `${h1}%`}}></div>
                      <div className="w-full bg-primary/80 rounded-t-sm group-hover:bg-primary transition-colors absolute bottom-0" style={{height: `${h2}%`}}></div>
                      <div className="absolute -bottom-6 w-full text-center text-[10px] font-label-sm text-on-surface-variant">{day}</div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-lg p-6 flex flex-col">
            <div className="flex justify-between items-center mb-6">
              <h3 className="font-headline-md text-headline-md text-on-surface">Demographic</h3>
              <button className="text-on-surface-variant hover:text-primary transition-colors">
                <span className="material-symbols-outlined text-[20px]">more_horiz</span>
              </button>
            </div>
            <div className="flex-1 flex flex-col gap-4">
              <div className="flex items-center justify-between group">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded bg-surface-container-highest flex items-center justify-center text-primary font-label-sm">#</div>
                  <div>
                    <div className="font-label-md text-label-md text-on-surface group-hover:text-primary transition-colors">reactjs</div>
                    <div className="font-label-sm text-label-sm text-on-surface-variant">1,204 active threads</div>
                  </div>
                </div>
                <div className="flex items-center gap-1 text-tertiary font-label-sm"><span className="material-symbols-outlined text-[14px]">arrow_upward</span><span>12%</span></div>
              </div>
              <div className="flex items-center justify-between group">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded bg-surface-container-highest flex items-center justify-center text-primary font-label-sm">#</div>
                  <div>
                    <div className="font-label-md text-label-md text-on-surface group-hover:text-primary transition-colors">python</div>
                    <div className="font-label-sm text-label-sm text-on-surface-variant">982 active threads</div>
                  </div>
                </div>
                <div className="flex items-center gap-1 text-tertiary font-label-sm"><span className="material-symbols-outlined text-[14px]">arrow_upward</span><span>8%</span></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
function AdminDashboardPage() {
  const { request } = useApi();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request('/api/admin/metrics').then(res => {
      if (mounted) { setData(res); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  if (loading || !data) return <div className="p-8 text-on-surface-variant flex justify-center mt-10"><Loader2 className="animate-spin mr-2"/> Loading dashboard...</div>;

  return (
    <div className="pt-8 pb-12 p-margin-mobile md:p-gutter max-w-container-max mx-auto">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-end mb-8 gap-4">
        <div>
          <h1 className="font-headline-xl text-headline-xl text-on-surface">Platform Overview</h1>
          <p className="text-on-surface-variant mt-2 max-w-2xl">Real-time metrics and moderation status for the DevStack community ecosystem.</p>
        </div>
        <div className="flex items-center gap-3">
          <span className="flex items-center gap-2 text-on-surface-variant font-label-sm text-label-sm bg-surface-container-highest px-3 py-1.5 rounded-full border border-outline-variant">
            <span className="w-2 h-2 rounded-full bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.6)]"></span>
            System Status: Healthy
          </span>
          <span className="font-label-sm text-label-sm text-on-surface-variant bg-surface-container px-3 py-1.5 rounded border border-outline-variant">
            Last sync: Just now
          </span>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl p-6 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-primary/10 transition-colors"></div>
          <div className="flex justify-between items-start mb-4 relative z-10">
            <div>
              <p className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Total Users</p>
              <h3 className="font-headline-lg text-headline-lg text-on-surface mt-1">{data.activeUserCount?.toLocaleString()}</h3>
            </div>
            <div className="p-2 bg-surface-container rounded-lg border border-outline-variant/50">
              <span className="material-symbols-outlined text-primary">group</span>
            </div>
          </div>
          <div className="flex items-center gap-2 mt-2 relative z-10">
            <span className="flex items-center text-green-400 font-label-sm text-label-sm bg-green-400/10 px-1.5 py-0.5 rounded">
              <span className="material-symbols-outlined text-[14px]">trending_up</span>
              +12.5%
            </span>
            <span className="text-on-surface-variant font-label-sm text-label-sm">vs last month</span>
          </div>
        </div>

        <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl p-6 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-tertiary/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-tertiary/10 transition-colors"></div>
          <div className="flex justify-between items-start mb-4 relative z-10">
            <div>
              <p className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Active Discussions</p>
              <h3 className="font-headline-lg text-headline-lg text-on-surface mt-1">{data.totalPostCount?.toLocaleString()}</h3>
            </div>
            <div className="p-2 bg-surface-container rounded-lg border border-outline-variant/50">
              <span className="material-symbols-outlined text-tertiary">forum</span>
            </div>
          </div>
          <div className="flex items-center gap-2 mt-2 relative z-10">
            <span className="flex items-center text-green-400 font-label-sm text-label-sm bg-green-400/10 px-1.5 py-0.5 rounded">
              <span className="material-symbols-outlined text-[14px]">trending_up</span>
              +5.2%
            </span>
            <span className="text-on-surface-variant font-label-sm text-label-sm">vs last month</span>
          </div>
        </div>

        <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl p-6 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-secondary/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-secondary/10 transition-colors"></div>
          <div className="flex justify-between items-start mb-4 relative z-10">
            <div>
              <p className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">Reports Pending</p>
              <h3 className="font-headline-lg text-headline-lg text-error mt-1">{data.flaggedContentCount?.toLocaleString()}</h3>
            </div>
            <div className="p-2 bg-surface-container rounded-lg border border-outline-variant/50">
              <span className="material-symbols-outlined text-secondary">report</span>
            </div>
          </div>
          <div className="flex items-center gap-2 mt-2 relative z-10">
            <span className="flex items-center text-error font-label-sm text-label-sm bg-error/10 px-1.5 py-0.5 rounded">
              <span className="material-symbols-outlined text-[14px]">trending_up</span>
              +18%
            </span>
            <span className="text-on-surface-variant font-label-sm text-label-sm">requires action</span>
          </div>
        </div>

        <div className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl p-6 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-primary-container/5 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:bg-primary-container/10 transition-colors"></div>
          <div className="flex justify-between items-start mb-4 relative z-10">
            <div>
              <p className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider">New Users</p>
              <h3 className="font-headline-lg text-headline-lg text-on-surface mt-1">{data.newUserCount?.toLocaleString()}</h3>
            </div>
            <div className="p-2 bg-surface-container rounded-lg border border-outline-variant/50">
              <span className="material-symbols-outlined text-primary-container">person_add</span>
            </div>
          </div>
          <div className="w-full bg-surface-container-high rounded-full h-1.5 mt-4 relative z-10">
            <div className="bg-primary-container h-1.5 rounded-full" style={{width: '65%'}}></div>
          </div>
        </div>
      </div>
    </div>
  );
}
function UserManagementPage() {
  const { request } = useApi();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('All');

  useEffect(() => {
    let mounted = true;
    request('/api/admin/users').then(res => {
      if (mounted) { setUsers(res.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const changeRole = (id, newRole) => {
    request(`/api/admin/users/${id}/role`, { method: 'PUT', body: JSON.stringify({ role: newRole }) })
      .then(res => setUsers(users.map(u => u.id === id ? { ...u, role: res.role } : u)));
  };

  const changeStatus = (id, newStatus) => {
    request(`/api/admin/users/${id}/status`, { method: 'PUT', body: JSON.stringify({ status: newStatus }) })
      .then(res => setUsers(users.map(u => u.id === id ? { ...u, status: res.status } : u)));
  };

  if (loading) return <div className="p-8 text-on-surface-variant flex justify-center mt-10"><Loader2 className="animate-spin mr-2"/> Loading users...</div>;

  const filteredUsers = users.filter(u => {
    if (filter === 'Admins') return u.role === 'Admin' || u.role === 'SuperAdmin';
    if (filter === 'Moderators') return u.role === 'Moderator';
    if (filter === 'Banned') return u.status === 'Banned';
    return true;
  });

  return (
    <div className="pt-8 pb-12 p-margin-mobile md:p-gutter max-w-[1400px] mx-auto">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-stack-md mb-stack-lg">
        <div>
          <h1 className="font-headline-xl text-headline-xl text-on-surface">User Directory</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-2 max-w-2xl">Manage platform access, monitor reputation metrics, and handle role assignments across the DevStack community.</p>
        </div>
        <div className="flex gap-3">
          <button className="flex items-center gap-2 px-4 py-2 bg-surface-container-high border border-outline-variant text-on-surface rounded font-label-md text-label-md hover:bg-surface-bright/50 transition-colors">
            <span className="material-symbols-outlined text-[18px]">download</span>
            Export CSV
          </button>
          <button className="flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded font-label-md text-label-md font-bold hover:bg-primary-container hover:text-on-primary-container transition-colors shadow-[0_0_15px_rgba(77,142,255,0.3)]">
            <span className="material-symbols-outlined text-[18px]">person_add</span>
            Invite User
          </button>
        </div>
      </div>

      <div className="bg-surface-container-lowest/80 backdrop-blur-xl border border-white/10 rounded-xl overflow-hidden shadow-[0_10px_30px_rgba(0,0,0,0.5)] flex flex-col">
        <div className="p-stack-md border-b border-white/10 bg-surface-container/50 flex flex-col sm:flex-row gap-4 justify-between items-center">
          <div className="flex gap-2 w-full sm:w-auto overflow-x-auto pb-2 sm:pb-0">
            {['All', 'Admins', 'Moderators', 'Banned'].map(f => (
              <button key={f} onClick={() => setFilter(f)} className={`px-3 py-1.5 rounded-lg font-label-sm text-label-sm whitespace-nowrap transition-colors ${filter === f ? 'bg-primary/10 border border-primary text-primary' : 'bg-transparent border border-outline-variant text-on-surface-variant hover:text-on-surface hover:bg-surface-bright/30'}`}>{f}</button>
            ))}
          </div>
          <div className="flex items-center gap-3 w-full sm:w-auto">
            <div className="relative flex-1 sm:w-64">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-on-surface-variant text-[18px]">filter_list</span>
              <select className="w-full bg-surface-dim border border-outline-variant rounded py-1.5 pl-9 pr-8 text-on-surface focus:border-primary focus:ring-1 focus:ring-primary focus:outline-none appearance-none font-label-sm text-label-sm">
                <option>Sort by Reputation (Desc)</option>
                <option>Sort by Reputation (Asc)</option>
                <option>Join Date (Newest)</option>
                <option>Join Date (Oldest)</option>
              </select>
            </div>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-white/10 bg-surface-container-low/50">
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">User</th>
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">Role</th>
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">Reputation</th>
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">Status</th>
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">Joined</th>
                <th className="py-3 px-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="font-body-md text-body-md divide-y divide-white/5">
              {filteredUsers.map(u => (
                <tr key={u.id} className={`hover:bg-surface-bright/20 transition-colors group ${u.status === 'Banned' ? 'bg-error/5' : ''}`}>
                  <td className={`py-3 px-4 ${u.status === 'Banned' ? 'opacity-60' : ''}`}>
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded border border-white/10 bg-surface-container-highest flex items-center justify-center font-headline-md text-primary font-bold">
                        {u.username.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <div className={`font-bold text-on-surface group-hover:text-primary transition-colors cursor-pointer ${u.status === 'Banned' ? 'line-through decoration-error/50' : ''}`}>{u.username}</div>
                        <div className="text-on-surface-variant font-label-sm text-label-sm">{u.email}</div>
                      </div>
                    </div>
                  </td>
                  <td className={`py-3 px-4 ${u.status === 'Banned' ? 'opacity-60' : ''}`}>
                    <select value={u.role} onChange={(e) => changeRole(u.id, e.target.value)} className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-[11px] font-label-sm font-bold bg-transparent border-0 focus:ring-0 cursor-pointer ${u.role === 'Admin' || u.role === 'SuperAdmin' ? 'text-secondary' : u.role === 'Moderator' ? 'text-primary' : 'text-on-surface-variant'}`}>
                      <option value="User" className="bg-surface-container text-on-surface">User</option>
                      <option value="Moderator" className="bg-surface-container text-on-surface">Moderator</option>
                      <option value="Admin" className="bg-surface-container text-on-surface">Admin</option>
                    </select>
                  </td>
                  <td className={`py-3 px-4 ${u.status === 'Banned' ? 'opacity-60' : ''}`}>
                    <div className="flex items-center gap-2">
                      <div className="w-16 h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                        <div className={`h-full ${u.reputation < 0 ? 'bg-error' : 'bg-primary'} w-[${Math.min(Math.abs(u.reputation) / 100, 100)}%]`}></div>
                      </div>
                      <span className={`font-label-sm text-label-sm ${u.reputation < 0 ? 'text-error' : 'text-on-surface'}`}>{u.reputation}</span>
                    </div>
                  </td>
                  <td className="py-3 px-4">
                    <select value={u.status} onChange={(e) => changeStatus(u.id, e.target.value)} className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-label-sm bg-transparent border-0 focus:ring-0 cursor-pointer ${u.status === 'Active' ? 'text-tertiary' : u.status === 'Banned' ? 'text-error' : 'text-on-surface-variant'}`}>
                      <option value="Active" className="bg-surface-container text-on-surface">Active</option>
                      <option value="Locked" className="bg-surface-container text-on-surface">Locked</option>
                      <option value="Banned" className="bg-surface-container text-on-surface">Banned</option>
                    </select>
                  </td>
                  <td className={`py-3 px-4 text-on-surface-variant font-label-sm text-label-sm ${u.status === 'Banned' ? 'opacity-60' : ''}`}>
                    {new Date(u.createdAt).toLocaleDateString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    <button className="p-1.5 text-on-surface-variant hover:text-primary hover:bg-primary/10 rounded transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100">
                      <span className="material-symbols-outlined text-[20px]">more_vert</span>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
function ModerationQueuePage() {
  const { request } = useApi();
  const [queue, setQueue] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    request('/api/moderation/queue').then(res => {
      if (mounted) { setQueue(res.items || []); setLoading(false); }
    }).catch(() => {
      if (mounted) setLoading(false);
    });
    return () => { mounted = false; };
  }, [request]);

  const handleResolve = (id, action) => {
    request('/api/moderation/resolve', {
      method: 'POST',
      body: JSON.stringify({ reportId: id, action })
    }).then(() => {
      setQueue(queue.filter(q => q.id !== id));
    });
  };

  if (loading) return <div className="p-8 text-on-surface-variant flex justify-center mt-10"><Loader2 className="animate-spin mr-2"/> Loading moderation queue...</div>;

  return (
    <div className="pt-8 pb-12 p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full flex flex-col gap-stack-lg">
      <header className="flex flex-col md:flex-row md:items-end justify-between gap-stack-md border-b border-white/10 pb-stack-md">
        <div>
          <h1 className="font-headline-lg-mobile md:font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface">Moderation Queue</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-1 max-w-2xl">Review reported content, manage user standing, and maintain community standards.</p>
        </div>
        <div className="flex items-center gap-stack-sm">
          <div className="bg-surface-container-high rounded-lg p-1 border border-outline-variant flex">
            <button className="px-3 py-1.5 rounded bg-surface-bright text-on-surface font-label-sm text-label-sm shadow-sm transition-colors">Pending ({queue.length})</button>
            <button className="px-3 py-1.5 rounded text-on-surface-variant hover:text-on-surface font-label-sm text-label-sm transition-colors">Resolved</button>
          </div>
          <button className="p-2 border border-outline-variant rounded-lg bg-surface-container-high hover:bg-surface-bright text-on-surface-variant transition-colors group">
            <span className="material-symbols-outlined group-hover:text-primary text-[20px]">filter_list</span>
          </button>
        </div>
      </header>

      {queue.length === 0 ? (
        <div className="bg-surface-container-lowest/80 backdrop-blur-xl border border-white/10 rounded-xl p-12 flex flex-col items-center justify-center">
          <span className="material-symbols-outlined text-[48px] text-on-surface-variant mb-4">check_circle</span>
          <h2 className="font-headline-md text-headline-md text-on-surface mb-2">Queue is empty</h2>
          <p className="font-body-md text-body-md text-on-surface-variant">All reported content has been reviewed.</p>
        </div>
      ) : (
        <div className="flex flex-col gap-stack-md">
          {queue.map(item => (
            <div key={item.id} className="grid grid-cols-1 lg:grid-cols-3 gap-stack-md">
              <article className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl lg:col-span-2 flex flex-col overflow-hidden relative group">
                <div className={`absolute top-0 left-0 w-1 h-full ${item.severity === 'High' ? 'bg-error' : 'bg-secondary'}`}></div>
                <div className="p-stack-md border-b border-white/5 bg-surface-container-low/50 flex justify-between items-center">
                  <div className="flex items-center gap-3">
                    <span className={`inline-flex items-center gap-1 px-2 py-1 rounded font-label-sm text-label-sm ${item.severity === 'High' ? 'bg-error-container/20 border border-error-container text-error' : 'bg-secondary-container/20 border border-secondary-container text-secondary'}`}>
                      <span className="material-symbols-outlined text-[14px]">warning</span>
                      {item.severity} Priority: {item.reason}
                    </span>
                    <span className="font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1">
                      <span className="material-symbols-outlined text-[14px]">timer</span> {new Date(item.reportedAt).toLocaleDateString()}
                    </span>
                  </div>
                  <span className="font-label-sm text-label-sm text-on-surface-variant">ID: #{item.id}</span>
                </div>
                
                <div className="p-stack-md flex-1 flex flex-col gap-stack-sm">
                  <h3 className="font-headline-md text-headline-md text-on-surface">Reported {item.contentType}</h3>
                  <div className="bg-surface-container-lowest p-stack-md rounded border border-outline-variant/30 font-body-md text-body-md text-on-surface font-mono text-sm relative">
                    <div className="absolute left-0 top-0 bottom-0 w-[2px] bg-outline-variant/50"></div>
                    <p className="pl-3">{item.contentExcerpt}</p>
                  </div>
                  
                  <div className="mt-auto pt-stack-md flex items-center gap-stack-md text-label-sm font-label-sm">
                    <div className="flex items-center gap-2">
                      <span className="text-on-surface-variant">Author:</span>
                      <a className="text-primary hover:underline flex items-center gap-1" href="#">@{item.author.username} {item.author.reputation < 0 && <span className="w-1.5 h-1.5 rounded-full bg-error inline-block"></span>}</a>
                      <span className="text-on-surface-variant text-xs">({item.author.reputation} rep)</span>
                    </div>
                    <div className="w-1 h-1 bg-outline-variant rounded-full"></div>
                    <div className="flex items-center gap-2">
                      <span className="text-on-surface-variant">Reporter:</span>
                      <a className="text-secondary hover:underline" href="#">@{item.reporter.username}</a>
                    </div>
                  </div>
                </div>
                
                <div className="p-stack-md border-t border-white/5 bg-surface-container-highest/30 flex justify-end gap-stack-sm">
                  <button onClick={() => handleResolve(item.id, 'Dismiss')} className="px-4 py-2 border border-outline-variant rounded-lg text-on-surface hover:bg-surface-bright font-label-md text-label-md transition-colors">
                    Dismiss
                  </button>
                  <button onClick={() => handleResolve(item.id, 'Warn')} className="px-4 py-2 border border-tertiary-container rounded-lg text-tertiary hover:bg-tertiary-container/20 font-label-md text-label-md transition-colors flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">warning</span>
                    Warn User
                  </button>
                  <button onClick={() => handleResolve(item.id, 'Delete')} className="px-4 py-2 bg-error text-on-error rounded-lg hover:bg-error/90 font-label-md text-label-md transition-colors flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">delete</span>
                    Delete Content
                  </button>
                </div>
              </article>
              
              <aside className="bg-surface-container/40 backdrop-blur-[12px] border border-white/10 rounded-xl lg:col-span-1 p-stack-md flex flex-col gap-stack-md">
                <h4 className="font-label-md text-label-md text-on-surface uppercase tracking-wider text-on-surface-variant border-b border-white/10 pb-2">Context: @{item.author.username}</h4>
                <div className="flex flex-col gap-stack-sm">
                  <div className="flex justify-between items-center">
                    <span className="font-label-sm text-label-sm text-on-surface-variant">Account Age</span>
                    <span className="font-label-sm text-label-sm text-on-surface">{new Date(item.author.joinedAt).toLocaleDateString()}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="font-label-sm text-label-sm text-on-surface-variant">Community Trust Score</span>
                    <span className={`font-label-sm text-label-sm ${item.author.reputation < 0 ? 'text-error' : 'text-primary'}`}>
                      {item.author.reputation}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="font-label-sm text-label-sm text-on-surface-variant">Recent Reports</span>
                    <span className="font-label-sm text-label-sm text-on-surface">{item.author.recentReports} in last 30 days</span>
                  </div>
                </div>
                <div className="mt-auto">
                  <button onClick={() => handleResolve(item.id, 'Suspend')} className="w-full py-2 border border-error-container text-error rounded-lg hover:bg-error-container/10 font-label-md text-label-md transition-colors flex items-center justify-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">block</span>
                    Suspend Account
                  </button>
                </div>
              </aside>
            </div>
          ))}
        </div>
      )}
    </div>
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
    <div className="flex-1 p-margin-mobile md:p-gutter overflow-y-auto h-full flex gap-gutter">
      <div className="flex-1 max-w-4xl mx-auto w-full">
        <header className="mb-stack-lg sticky top-0 bg-background/90 backdrop-blur-md z-10 pb-4 pt-2 -mt-2">
          <div className="flex justify-between items-end mb-4">
            <h1 className="font-headline-lg md:font-headline-xl text-headline-lg md:text-headline-xl text-on-surface">Your Feed</h1>
            <Link to="/articles/new" className="hidden sm:flex items-center gap-2 bg-primary text-on-primary px-4 py-2 rounded-lg font-label-md hover:bg-primary/90 transition-colors">
              <span className="material-symbols-outlined text-[18px]">add</span>
              New Post
            </Link>
          </div>
          <div className="flex flex-wrap gap-4 border-b border-white/10 font-label-md text-label-md items-center">
            {SORT_OPTIONS.map((option) => (
              <button
                className={`px-2 py-3 ${filters.sort === option.value ? "text-primary border-b-2 border-primary font-bold" : "text-on-surface-variant hover:text-on-surface transition-colors"}`}
                key={option.value}
                type="button"
                onClick={() => updateFilter("sort", option.value)}
              >
                {option.label}
              </button>
            ))}
            <div className="ml-auto flex items-center py-2 relative">
              <span className="material-symbols-outlined absolute left-2 text-[18px] text-on-surface-variant">search</span>
              <input
                className="bg-surface-dim border border-white/10 rounded-md pl-8 pr-2 py-1.5 text-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary text-on-surface w-full sm:w-64 placeholder-on-surface-variant"
                value={filters.tag}
                onChange={(event) => updateFilter("tag", event.target.value)}
                placeholder="Filter by tag..."
              />
            </div>
          </div>
        </header>

        <AsyncState loading={loading} error={error}>
          <div className="flex flex-col gap-6 pb-12">
            {asItems(articles).map(article => (
              <article key={article.id} className="bg-surface-variant/40 border border-white/5 rounded-xl p-5 relative overflow-hidden group hover:border-primary/30 transition-colors shadow-sm hover:shadow-md">
                <div className="absolute top-0 left-0 w-1 h-full bg-tertiary-container opacity-0 group-hover:opacity-100 transition-opacity"></div>
                <div className="flex justify-between items-start mb-3">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-secondary-container/20 flex items-center justify-center text-secondary font-bold border border-secondary/20">
                      {article.author?.username?.substring(0,2).toUpperCase() || 'U'}
                    </div>
                    <div>
                      <h3 className="font-label-md text-label-md text-on-surface font-bold">{article.author?.displayName || article.author?.username || 'Unknown'}</h3>
                      <p className="font-label-sm text-label-sm text-on-surface-variant">{formatDate(article.createdAt)}</p>
                    </div>
                  </div>
                </div>
                <Link to={`/articles/${article.slug}`}>
                  <h2 className="font-headline-md text-headline-md text-on-surface mb-2 mt-2 group-hover:text-primary transition-colors cursor-pointer line-clamp-2">
                    {article.title}
                  </h2>
                </Link>
                <p className="font-body-md text-body-md text-on-surface-variant mb-4 line-clamp-3">
                  {article.body?.substring(0, 200)}...
                </p>
                <div className="flex flex-wrap gap-2 mb-4">
                  {article.tags?.map(tag => (
                    <span key={tag} className="px-2.5 py-1 rounded bg-surface-container text-tertiary font-label-sm text-label-sm border border-white/5">{tag}</span>
                  ))}
                </div>
                <div className="flex items-center gap-6 mt-4 pt-4 border-t border-white/5">
                  <div className="flex items-center gap-2 text-on-surface-variant font-label-md text-label-md">
                    <span className="material-symbols-outlined text-[18px]">arrow_upward</span>
                    {article.voteCount || 0}
                  </div>
                  <div className="flex items-center gap-2 text-on-surface-variant font-label-md text-label-md">
                    <span className="material-symbols-outlined text-[18px]">chat_bubble</span>
                    Discussion
                  </div>
                </div>
              </article>
            ))}
            {asItems(articles).length === 0 && (
              <div className="text-center py-12 text-on-surface-variant">
                <span className="material-symbols-outlined text-4xl mb-2 opacity-50">article</span>
                <p>No articles found.</p>
              </div>
            )}
          </div>
          <div className="flex justify-center pb-8">
            <Pagination
              page={filters.page}
              pageCount={pageCount}
              onPage={(page) => updateFilter("page", String(page))}
            />
          </div>
        </AsyncState>
      </div>
      
      <aside className="hidden lg:flex flex-col gap-6 w-[300px] xl:w-[340px] shrink-0 sticky top-24 h-fit">
        <Link to="/articles/new" className="w-full bg-primary text-on-primary flex items-center justify-center gap-2 py-3 rounded-xl font-label-md hover:bg-primary-fixed-dim transition-colors shadow-lg shadow-primary/20">
          <span className="material-symbols-outlined text-[20px]">edit_document</span>
          Start a Discussion
        </Link>
        <div className="bg-surface-variant/40 backdrop-blur-sm border border-white/5 rounded-xl p-5">
          <div className="flex items-center justify-between mb-4 pb-3 border-b border-white/5">
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-secondary">auto_awesome</span>
              <h3 className="font-headline-md text-[18px] text-on-surface">For You</h3>
            </div>
          </div>
          {api.isAuthenticated ? (
            recResource.loading ? (
              <div className="text-on-surface-variant text-sm py-4 text-center flex items-center justify-center gap-2">
                <span className="material-symbols-outlined animate-spin">refresh</span>
                Loading...
              </div>
            ) : recResource.isEmpty ? (
              <div className="text-on-surface-variant text-sm py-4 text-center">No recommendations yet.</div>
            ) : recResource.error ? (
              <div className="text-error text-sm p-3 bg-error-container/10 rounded">{recResource.error}</div>
            ) : (
              <div className="flex flex-col gap-1">
                {asItems(recResource.data).map((rec) => (
                  <Link key={rec.id} to={`/articles/${rec.slug}`} className="flex flex-col p-3 rounded-lg hover:bg-surface-container transition-colors group cursor-pointer border border-transparent hover:border-white/5">
                    <div className="font-label-md text-label-md text-on-surface group-hover:text-primary transition-colors line-clamp-2">{rec.title}</div>
                    <div className="font-label-sm text-label-sm text-on-surface-variant mt-1.5 flex items-center gap-1">
                      <span className="w-4 h-4 rounded-full bg-secondary-container/20 flex items-center justify-center text-secondary text-[10px] font-bold">
                        {rec.author?.username?.substring(0,1).toUpperCase() || 'U'}
                      </span>
                      {rec.author?.username}
                    </div>
                  </Link>
                ))}
              </div>
            )
          ) : (
            <div className="text-on-surface-variant text-sm p-4 bg-surface-container/50 rounded-lg text-center border border-white/5">
              Log in to see personalized picks.
            </div>
          )}
        </div>
      </aside>
    </div>
  );
}

function ArticleCollection({ api, mode }) {
  const [page, setPage] = useState(1);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const title = mode === "mine" ? "My articles" : "Saved Library";
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
    <main className="flex-1 p-gutter max-w-5xl mx-auto w-full">
      <div className="mb-stack-lg border-b border-white/10 pb-6 flex items-center justify-between">
        <div>
          <h1 className="font-headline-xl text-headline-xl text-on-surface mb-2">{title}</h1>
          <p className="font-body-md text-body-md text-on-surface-variant">
            {mode === "mine" ? "Your published articles and drafts." : "Your personal collection of bookmarked resources."}
          </p>
        </div>
        <button className="p-2 text-on-surface-variant hover:bg-surface-container hover:text-on-surface rounded-full transition-colors" type="button" onClick={load} title="Refresh">
          <span className="material-symbols-outlined text-[24px]">refresh</span>
        </button>
      </div>

      <AsyncState loading={loading} error={error}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 pb-12">
          {asItems(result).map(article => (
            <article key={article.id} className="bg-surface-variant/40 rounded-xl p-6 transition-all duration-300 relative overflow-hidden group hover:bg-surface-variant/60 hover:border-primary/30 border border-white/5 cursor-pointer shadow-sm hover:shadow-md">
              <Link to={`/articles/${article.slug}`} className="absolute inset-0 z-10"></Link>
              <div className="flex justify-between items-start mb-4">
                <div className="flex items-center gap-3">
                  <span className="bg-secondary-container/20 text-secondary font-label-sm text-label-sm px-2 py-1 rounded border border-secondary/20">{article.status || 'Published'}</span>
                  <span className="text-on-surface-variant font-label-sm text-label-sm flex items-center gap-1">
                    <span className="material-symbols-outlined text-[16px]">schedule</span> {formatDate(article.createdAt)}
                  </span>
                </div>
                {mode === "mine" && (
                  <Link to={`/articles/${article.slug}/edit`} className="relative z-20 text-on-surface-variant hover:text-primary transition-colors p-1 bg-surface rounded shadow-sm border border-white/5">
                    <span className="material-symbols-outlined text-[18px]">edit</span>
                  </Link>
                )}
              </div>
              <h2 className="font-headline-lg text-headline-md mb-3 group-hover:text-primary transition-colors text-on-surface line-clamp-2">{article.title}</h2>
              <div className="flex gap-2 flex-wrap mt-auto pt-4">
                {article.tags?.map(t => (
                  <span key={t} className="text-label-sm font-label-sm px-2 py-0.5 rounded border border-outline-variant text-on-surface-variant">#{t}</span>
                ))}
              </div>
            </article>
          ))}
          {asItems(result).length === 0 && (
            <div className="col-span-full py-12 text-center text-on-surface-variant border border-dashed border-white/10 rounded-xl bg-surface-container/20">
              <span className="material-symbols-outlined text-4xl mb-2 opacity-50">{mode === "mine" ? "edit_document" : "bookmark"}</span>
              <p>{mode === "mine" ? "You haven't written any articles yet." : "You haven't saved any articles yet."}</p>
              {mode === "mine" && (
                <Link to="/articles/new" className="inline-block mt-4 text-primary font-bold hover:underline">Start writing</Link>
              )}
            </div>
          )}
        </div>
        <div className="flex justify-center pb-8">
          <Pagination page={page} pageCount={pageCount} onPage={setPage} />
        </div>
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

  useEffect(() => { load(); }, [load]);

  const vote = async (targetId, targetType, value) => {
    if (!api.isAuthenticated) {
      navigate(`/auth?returnUrl=${encodeURIComponent(location.pathname)}`);
      return;
    }
    const isArticle = targetType === "Article";
    let previousVote = 0;
    let previousCount = 0;
    
    if (isArticle) {
      previousVote = article.userVote || 0;
      previousCount = article.voteCount || 0;
      const voteDiff = value - previousVote;
      setArticle(c => ({ ...c, userVote: value, voteCount: c.voteCount + voteDiff }));
    }
    
    setBusy(`${targetType}-${targetId}-${value}`);
    try {
      const result = await api.request("/api/votes", {
        method: "POST",
        body: JSON.stringify({ targetId, targetType, value }),
      });
      if (isArticle) {
        setArticle(c => ({
          ...c,
          voteCount: getField(result, "newVoteCount", c.voteCount),
          userVote: getField(result, "userVote", value),
        }));
      } else {
        await commentsResource.reload();
      }
    } catch (voteError) {
      if (isArticle) {
        setArticle(c => ({ ...c, userVote: previousVote, voteCount: previousCount }));
      }
      setError(voteError.message);
    } finally {
      setBusy("");
    }
  };

  const toggleBookmark = async () => {
    if (!api.isAuthenticated) {
      navigate(`/auth?returnUrl=${encodeURIComponent(location.pathname)}`);
      return;
    }
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
    if (!api.isAuthenticated) {
      navigate(`/auth?returnUrl=${encodeURIComponent(location.pathname)}`);
      return;
    }
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
    <main className="flex-1 p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full flex flex-col lg:flex-row gap-gutter">
      <AsyncState loading={loading} error={error}>
        {article && (
          <>
            <div className="hidden lg:flex flex-col items-center gap-2 sticky top-24 h-fit w-16 pt-2 shrink-0">
              <button onClick={() => vote(articleId, "Article", article.userVote === 1 ? 0 : 1)} className={`p-2 rounded-full hover:bg-surface-container transition-colors shadow-sm ${article.userVote === 1 ? 'text-primary bg-primary/10' : 'text-on-surface-variant'}`}>
                <span className="material-symbols-outlined text-[28px]" style={{ fontVariationSettings: article.userVote === 1 ? "'FILL' 1" : "'FILL' 0" }}>arrow_upward</span>
              </button>
              <span className={`font-headline-md text-headline-md font-bold ${article.userVote > 0 ? 'text-primary' : article.userVote < 0 ? 'text-error' : 'text-on-surface'}`}>{article.voteCount || 0}</span>
              <button onClick={() => vote(articleId, "Article", article.userVote === -1 ? 0 : -1)} className={`p-2 rounded-full hover:bg-surface-container transition-colors shadow-sm ${article.userVote === -1 ? 'text-error bg-error/10' : 'text-on-surface-variant'}`}>
                <span className="material-symbols-outlined text-[28px]" style={{ fontVariationSettings: article.userVote === -1 ? "'FILL' 1" : "'FILL' 0" }}>arrow_downward</span>
              </button>
              <div className="w-8 h-px bg-white/10 my-3"></div>
              <button onClick={toggleBookmark} className={`p-2 rounded-full hover:bg-surface-container transition-colors shadow-sm ${article.bookmarked ? 'text-tertiary bg-tertiary/10' : 'text-on-surface-variant'}`} title="Bookmark">
                <span className="material-symbols-outlined text-[24px]" style={{ fontVariationSettings: article.bookmarked ? "'FILL' 1" : "'FILL' 0" }}>bookmark</span>
              </button>
            </div>
            
            <div className="flex-1 w-full max-w-[800px] mx-auto lg:mx-0 bg-surface-container-lowest/50 rounded-2xl p-4 md:p-8 border border-white/5 shadow-xl">
              <header className="mb-stack-lg border-b border-white/10 pb-8">
                <div className="flex flex-wrap items-center justify-between gap-4 mb-4">
                  <div className="flex flex-wrap gap-2">
                    {article.tags?.map(t => (
                      <span key={t} className="px-3 py-1 rounded-full bg-secondary-container/20 text-secondary border border-secondary/20 font-label-sm text-label-sm">{t}</span>
                    ))}
                  </div>
                  <span className="text-on-surface-variant font-label-sm text-label-sm flex items-center gap-1 bg-surface-container px-3 py-1 rounded-full">
                    <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                    {formatDate(article.publishedAt || article.createdAt)}
                  </span>
                </div>
                <h1 className="font-headline-xl text-headline-lg-mobile md:text-headline-xl text-on-surface mb-8 leading-tight tracking-tight">
                  {article.title}
                </h1>
                
                {article.coverImageUrl && (
                  <img src={article.coverImageUrl} alt="Cover" className="w-full h-auto max-h-[400px] object-cover rounded-xl mb-8 border border-white/5 shadow-md" />
                )}
                
                <div className="flex items-center gap-4 p-4 rounded-xl bg-surface-variant/40 border border-white/5 shadow-inner">
                  <div className="w-12 h-12 rounded-full bg-secondary-container/20 flex items-center justify-center text-secondary font-headline-md text-xl border border-secondary/30 shadow-sm">
                    {article.author?.username?.substring(0,2).toUpperCase() || 'U'}
                  </div>
                  <div className="flex flex-col">
                    <span className="font-label-md text-label-md text-on-surface-variant">Written by</span>
                    <div className="font-headline-sm text-lg text-on-surface font-bold">{article.author?.displayName || article.author?.username}</div>
                  </div>
                  {article.author?.username === api.token && (
                    <div className="ml-auto flex gap-2">
                      <Link to={`/articles/${slug}/edit`} className="bg-surface-container text-on-surface-variant hover:text-primary transition-colors p-2 rounded-lg border border-white/5 hover:border-primary/30"><span className="material-symbols-outlined text-[20px]">edit</span></Link>
                      <button onClick={deleteArticle} className="bg-surface-container text-on-surface-variant hover:text-error transition-colors p-2 rounded-lg border border-white/5 hover:border-error/30"><span className="material-symbols-outlined text-[20px]">delete</span></button>
                    </div>
                  )}
                </div>
              </header>
              
              <article className="prose prose-invert prose-lg max-w-none text-on-surface-variant font-body-lg text-body-lg mb-stack-lg pb-stack-lg border-b border-white/10 leading-relaxed">
                <div dangerouslySetInnerHTML={{ __html: article.body?.replace(/\n/g, '<br/>') || '' }} />
              </article>
              
              <section className="mt-12" id="comments">
                <div className="flex items-center gap-3 mb-8">
                  <span className="material-symbols-outlined text-3xl text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>forum</span>
                  <h3 className="font-headline-lg text-headline-lg text-on-surface">
                    Discussion ({Array.isArray(commentsResource.data) ? commentsResource.data.length : 0})
                  </h3>
                </div>
                
                <div className="mb-10 bg-surface-container/30 border border-white/10 p-5 rounded-2xl shadow-sm">
                  <form onSubmit={submitComment}>
                    {replyTo && (
                      <div className="text-sm text-primary mb-3 flex justify-between items-center bg-primary/10 px-3 py-2 rounded-lg border border-primary/20">
                        <span>Replying to comment...</span>
                        <button type="button" onClick={() => setReplyTo(null)} className="hover:bg-primary/20 p-1 rounded-full"><span className="material-symbols-outlined text-[16px]">close</span></button>
                      </div>
                    )}
                    <div className="flex items-start gap-4">
                      <div className="hidden sm:flex w-10 h-10 rounded-full bg-surface-variant flex-shrink-0 items-center justify-center border border-white/10 mt-1">
                         <span className="material-symbols-outlined text-on-surface-variant">person</span>
                      </div>
                      <div className="flex-1 flex flex-col gap-3">
                        <textarea 
                          className="w-full bg-surface-dim border border-outline-variant/30 rounded-xl p-4 font-body-md text-body-md text-on-surface placeholder:text-on-surface-variant/50 focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/50 transition-all min-h-[120px] resize-y shadow-inner" 
                          placeholder={api.isAuthenticated ? "Share your thoughts or ask a question..." : "Log in to join the discussion"}
                          value={commentBody}
                          onChange={(e) => setCommentBody(e.target.value)}
                          disabled={!api.isAuthenticated}
                        />
                        <div className="flex justify-end">
                          <button disabled={busy === "comment" || !api.isAuthenticated || !commentBody.trim()} className="bg-primary text-on-primary font-label-md text-label-md px-6 py-2.5 rounded-lg hover:bg-primary-fixed-dim transition-all shadow-md shadow-primary/20 disabled:opacity-50 disabled:shadow-none flex items-center gap-2 font-bold">
                            {busy === "comment" ? (
                              <><span className="material-symbols-outlined animate-spin text-[18px]">progress_activity</span> Posting</>
                            ) : (
                              <><span className="material-symbols-outlined text-[18px]">send</span> Post Comment</>
                            )}
                          </button>
                        </div>
                      </div>
                    </div>
                  </form>
                </div>
                
                <div className="space-y-8">
                  {commentsResource.loading ? (
                    <div className="flex flex-col items-center justify-center py-12 text-on-surface-variant gap-3">
                      <span className="material-symbols-outlined animate-spin text-3xl">refresh</span>
                      <span>Loading comments...</span>
                    </div>
                  ) : commentsResource.isEmpty ? (
                    <div className="text-on-surface-variant text-center py-12 bg-surface-container/20 rounded-xl border border-dashed border-white/10">
                      <span className="material-symbols-outlined text-4xl mb-3 opacity-50">speaker_notes_off</span>
                      <p>No comments yet. Be the first to share your thoughts!</p>
                    </div>
                  ) : (
                    (Array.isArray(commentsResource.data) ? commentsResource.data : []).map(comment => (
                      <div key={comment.id} className="comment-group relative flex gap-4">
                        {comment.replies?.length > 0 && (
                          <div className="thread-line w-px bg-white/10 absolute top-12 bottom-0 left-5"></div>
                        )}
                        <div className="flex flex-col items-center z-10 bg-transparent shrink-0">
                          <div className="w-10 h-10 rounded-full bg-secondary-container/20 flex items-center justify-center text-secondary font-bold border border-secondary/30 text-sm shadow-sm">
                            {comment.author?.username?.substring(0,2).toUpperCase() || 'U'}
                          </div>
                        </div>
                        <div className="flex-1 pb-2">
                          <div className="bg-surface-variant/30 rounded-xl p-4 border border-white/5">
                            <div className="flex items-center gap-2 mb-2">
                              <span className="font-label-md text-label-md font-bold text-on-surface">{comment.author?.displayName || comment.author?.username}</span>
                              <span className="font-label-sm text-label-sm text-on-surface-variant ml-2 flex items-center gap-1">
                                <span className="material-symbols-outlined text-[12px]">schedule</span>
                                {formatDate(comment.createdAt)}
                              </span>
                              {comment.author?.username === api.token && (
                                <button onClick={() => removeComment(comment.id)} className="ml-auto text-on-surface-variant hover:text-error transition-colors"><span className="material-symbols-outlined text-[16px]">delete</span></button>
                              )}
                            </div>
                            <p className="font-body-md text-body-md text-on-surface-variant mb-4 leading-relaxed">
                              {comment.body}
                            </p>
                            <div className="flex items-center gap-6 text-on-surface-variant font-label-sm text-label-sm pt-2 border-t border-white/5">
                              <div className="flex items-center gap-1 bg-surface-container rounded-full px-2 py-1">
                                <button onClick={() => vote(comment.id, "Comment", comment.userVote === 1 ? 0 : 1)} className={`hover:text-primary transition-colors flex items-center justify-center p-1 rounded-full hover:bg-white/5 ${comment.userVote === 1 ? 'text-primary' : ''}`}><span className="material-symbols-outlined text-[16px]">arrow_upward</span></button>
                                <span className={`font-bold w-4 text-center ${comment.userVote > 0 ? 'text-primary' : comment.userVote < 0 ? 'text-error' : 'text-on-surface'}`}>{comment.voteCount || 0}</span>
                                <button onClick={() => vote(comment.id, "Comment", comment.userVote === -1 ? 0 : -1)} className={`hover:text-error transition-colors flex items-center justify-center p-1 rounded-full hover:bg-white/5 ${comment.userVote === -1 ? 'text-error' : ''}`}><span className="material-symbols-outlined text-[16px]">arrow_downward</span></button>
                              </div>
                              <button onClick={() => { setReplyTo(comment.id); document.getElementById('comments')?.scrollIntoView({behavior: 'smooth'}); }} className="hover:text-on-surface hover:bg-surface-container px-3 py-1.5 rounded-full transition-colors flex items-center gap-1.5 font-bold">
                                <span className="material-symbols-outlined text-[16px]">reply</span> Reply
                              </button>
                            </div>
                          </div>
                          
                          {comment.replies?.length > 0 && (
                            <div className="mt-4 space-y-4">
                              {comment.replies.map(reply => (
                                <div key={reply.id} className="flex gap-4">
                                  <div className="flex flex-col items-center z-10 shrink-0 mt-1">
                                    <div className="w-8 h-8 rounded-full bg-surface-container flex items-center justify-center text-on-surface-variant font-bold border border-white/10 text-xs">
                                      {reply.author?.username?.substring(0,2).toUpperCase() || 'U'}
                                    </div>
                                  </div>
                                  <div className="flex-1 bg-surface-container-lowest/80 rounded-xl p-3 border border-white/5">
                                    <div className="flex items-center gap-2 mb-1">
                                      <span className="font-label-md text-label-md font-bold text-on-surface">{reply.author?.displayName || reply.author?.username}</span>
                                      <span className="font-label-sm text-label-sm text-on-surface-variant ml-2">{formatDate(reply.createdAt)}</span>
                                      {reply.author?.username === api.token && (
                                        <button onClick={() => removeComment(reply.id)} className="ml-auto text-on-surface-variant hover:text-error transition-colors"><span className="material-symbols-outlined text-[14px]">delete</span></button>
                                      )}
                                    </div>
                                    <p className="font-body-md text-body-md text-on-surface-variant">{reply.body}</p>
                                  </div>
                                </div>
                              ))}
                            </div>
                          )}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </section>
            </div>
            
            <div className="lg:hidden fixed bottom-6 right-6 flex flex-col gap-2 z-40">
              <button onClick={toggleBookmark} className={`w-12 h-12 rounded-full shadow-lg flex items-center justify-center border border-white/10 backdrop-blur-md ${article.bookmarked ? 'bg-tertiary text-on-tertiary' : 'bg-surface-container-high text-on-surface'}`}>
                <span className="material-symbols-outlined" style={{ fontVariationSettings: article.bookmarked ? "'FILL' 1" : "'FILL' 0" }}>bookmark</span>
              </button>
            </div>
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
    <main className="flex-1 p-margin-mobile md:p-gutter max-w-container-max mx-auto w-full">
      <AsyncState loading={loading} error={error}>
        <form onSubmit={submit} className="flex flex-col lg:flex-row gap-gutter relative">
          <div className="flex-1 flex flex-col gap-stack-md max-w-[800px]">
            <div className="flex items-center justify-between border-b border-white/10 pb-4 mb-4">
              <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface">{mode === "edit" ? "Update article" : "Create a new post"}</h1>
            </div>
            
            <div className="bg-surface-container/60 backdrop-blur-xl border border-white/10 rounded-2xl flex flex-col overflow-hidden shadow-xl">
              <div className="p-6 border-b border-white/5">
                <input
                  required
                  maxLength={200}
                  className="w-full bg-transparent border-none outline-none font-headline-md text-headline-md text-on-surface placeholder:text-on-surface-variant/50 focus:ring-0 p-0"
                  value={form.title}
                  onChange={(event) => setForm({ ...form, title: event.target.value })}
                  placeholder="Post title..."
                />
              </div>
              
              <div className="p-4 border-b border-white/5 flex items-center gap-2 bg-surface-container-lowest/30">
                <span className="material-symbols-outlined text-on-surface-variant text-[18px] ml-2">sell</span>
                <input
                  required
                  className="w-full bg-transparent border-none outline-none font-label-md text-label-md text-on-surface placeholder:text-on-surface-variant/50 focus:ring-0 p-1"
                  value={form.tags}
                  onChange={(event) => setForm({ ...form, tags: event.target.value })}
                  placeholder="Tags (comma separated)... e.g. react, nextjs, typescript"
                />
              </div>

              <div className="bg-surface-container px-4 py-3 border-b border-white/5 flex items-center gap-1 overflow-x-auto">
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Bold"><span className="material-symbols-outlined text-[20px]">format_bold</span></button>
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Italic"><span className="material-symbols-outlined text-[20px]">format_italic</span></button>
                <div className="w-[1px] h-5 bg-white/10 mx-2"></div>
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Link"><span className="material-symbols-outlined text-[20px]">link</span></button>
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Image"><span className="material-symbols-outlined text-[20px]">image</span></button>
                <div className="w-[1px] h-5 bg-white/10 mx-2"></div>
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Code Block"><span className="material-symbols-outlined text-[20px]">code_blocks</span></button>
                <button type="button" className="p-1.5 text-on-surface-variant hover:bg-surface-bright hover:text-on-surface rounded transition-colors flex items-center justify-center" title="Quote"><span className="material-symbols-outlined text-[20px]">format_quote</span></button>
              </div>

              <div className="relative min-h-[400px] flex flex-col bg-surface-container-lowest/50">
                <textarea
                  required
                  maxLength={50000}
                  className="w-full flex-1 bg-transparent border-none outline-none p-6 font-body-lg text-body-lg text-on-surface placeholder:text-on-surface-variant/40 resize-y min-h-[400px] focus:ring-0 focus:outline-none leading-relaxed"
                  value={form.body}
                  onChange={(event) => setForm({ ...form, body: event.target.value })}
                  placeholder="Write your post content here... Markdown is fully supported."
                />
              </div>
              
              <div className="p-5 bg-surface-container-high border-t border-white/5 flex flex-wrap items-center justify-between gap-4">
                <div className="flex items-center gap-6">
                  <label className="flex items-center gap-2 text-on-surface-variant font-label-md">
                    Status:
                    <select 
                      className="bg-surface-dim border border-white/10 rounded-lg px-3 py-1.5 text-on-surface focus:outline-none focus:border-primary font-bold shadow-sm"
                      value={form.status} 
                      onChange={(event) => setForm({ ...form, status: event.target.value })}
                    >
                      <option value="Draft">Draft</option>
                      <option value="Published">Published</option>
                    </select>
                  </label>
                  {mode === "edit" && (
                    <label className="flex items-center gap-2 text-on-surface-variant font-label-md hidden sm:flex">
                      Cover ID:
                      <input 
                        className="bg-surface-dim border border-white/10 rounded-lg px-3 py-1.5 text-on-surface focus:outline-none focus:border-primary w-32 shadow-sm text-sm"
                        value={form.coverImageKey} 
                        onChange={(event) => setForm({ ...form, coverImageKey: event.target.value })}
                        placeholder="Image key..."
                      />
                    </label>
                  )}
                </div>
                <button type="submit" disabled={saving} className="px-8 py-2.5 bg-primary text-on-primary rounded-lg font-label-md text-label-md hover:bg-primary-fixed-dim transition-all shadow-lg shadow-primary/20 flex items-center gap-2 font-bold disabled:opacity-50">
                  {saving ? (
                    <><span className="material-symbols-outlined animate-spin text-[18px]">progress_activity</span> Saving</>
                  ) : (
                    <>{mode === "edit" ? "Update Post" : "Publish Post"} <span className="material-symbols-outlined text-[18px]">send</span></>
                  )}
                </button>
              </div>
            </div>
          </div>
          
          <div className="w-full lg:w-[300px] flex flex-col gap-stack-md shrink-0">
            <div className="bg-surface-variant/40 backdrop-blur-md border border-white/10 rounded-2xl p-6 sticky top-24 shadow-lg">
              <h3 className="font-headline-md text-headline-md text-on-surface flex items-center gap-2 mb-6 pb-4 border-b border-white/5">
                <span className="material-symbols-outlined text-tertiary">rule</span>
                Posting Rules
              </h3>
              <ul className="space-y-6 font-body-md text-body-md text-on-surface-variant text-sm">
                <li className="flex items-start gap-4">
                  <span className="bg-primary-container/20 text-primary w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0 mt-0.5 border border-primary/20">1</span>
                  <div><strong className="text-on-surface block mb-1 text-base">Be specific</strong>Provide clear context, error messages, and what you've already tried.</div>
                </li>
                <li className="flex items-start gap-4">
                  <span className="bg-primary-container/20 text-primary w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0 mt-0.5 border border-primary/20">2</span>
                  <div><strong className="text-on-surface block mb-1 text-base">Format code</strong>Use markdown code blocks with language tags for readability.</div>
                </li>
                <li className="flex items-start gap-4">
                  <span className="bg-primary-container/20 text-primary w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0 mt-0.5 border border-primary/20">3</span>
                  <div><strong className="text-on-surface block mb-1 text-base">Be professional</strong>Maintain a constructive and respectful tone towards all members.</div>
                </li>
              </ul>
              
              <div className="mt-8 pt-6 border-t border-white/5">
                <h4 className="font-label-md text-on-surface mb-3 flex items-center gap-2">
                  <span className="material-symbols-outlined text-[16px] text-on-surface-variant">markdown</span>
                  Markdown Tips
                </h4>
                <div className="bg-surface-container-lowest rounded-lg p-3 text-xs text-on-surface-variant font-mono space-y-2 border border-white/5">
                  <div>**bold** <span className="float-right font-bold text-on-surface font-sans">bold</span></div>
                  <div>*italic* <span className="float-right italic text-on-surface font-sans">italic</span></div>
                  <div>[link](url) <span className="float-right text-primary font-sans underline">link</span></div>
                  <div>`code` <span className="float-right bg-surface px-1 rounded text-primary">code</span></div>
                </div>
              </div>
            </div>
          </div>
        </form>
      </AsyncState>
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
