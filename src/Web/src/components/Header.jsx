import { Terminal, Share, Activity } from 'lucide-react';

const Header = ({ isConnected }) => {
  return (
    <header className="header animate-fade-in">
      <div className="header-title">
        <Terminal size={24} color="#a855f7" />
        <span>JudgeSync Pair</span>
      </div>
      
      <div className="header-actions">
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginRight: '16px' }}>
          <div className={`status-indicator ${!isConnected ? 'disconnected' : ''}`} />
          <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            {isConnected ? 'Connected' : 'Reconnecting...'}
          </span>
        </div>

        <button 
          className="btn" 
          onClick={() => {
            navigator.clipboard.writeText(window.location.href);
            alert("Invite link copied to clipboard!");
          }}
        >
          <Share size={16} />
          Invite
        </button>
        
        <button className="btn btn-primary" onClick={() => alert("Submitting code to Judge...")}>
          <Activity size={16} />
          Run & Submit
        </button>
      </div>
    </header>
  );
};

export default Header;
