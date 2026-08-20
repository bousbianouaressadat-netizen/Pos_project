import { useSession } from '../../services/SessionContext';
import './Topbar.css';

export default function Topbar() {
  const { user } = useSession();

  return (
    <header className="topbar">
      <div className="topbar__search">
        <input
          type="text"
          placeholder="بحث سريع... (منتج، عميل، فاتورة)"
          className="topbar__search-input"
          aria-label="بحث سريع"
        />
      </div>

      <div className="topbar__actions">
        <button className="topbar__icon-btn" aria-label="الإشعارات">
          🔔
        </button>

        <div className="topbar__user">
          <span className="topbar__user-name">{user?.fullName ?? 'زائر'}</span>
          <span className="topbar__user-role">{user?.roles.join(', ')}</span>
        </div>
      </div>
    </header>
  );
}
