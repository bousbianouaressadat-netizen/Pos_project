import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../../shared/services/authService';
import { useSession } from '../../shared/services/SessionContext';
import { ApiError } from '../../shared/services/apiClient';
import './LoginPage.css';

export default function LoginPage() {
  const { setUser } = useSession();
  const navigate = useNavigate();

  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const user = await login(username, password);
      setUser(user);
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'تعذّر الاتصال بالخادم — تحقق من الرابط بملف .env');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <form className="login-page__card" onSubmit={handleSubmit}>
        <h1 className="login-page__title">POS Business Manager</h1>
        <p className="login-page__subtitle">تسجيل الدخول</p>

        <label className="login-page__label">اسم المستخدم</label>
        <input
          className="login-page__input"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          autoFocus
        />

        <label className="login-page__label">كلمة السر</label>
        <input
          className="login-page__input"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        {error && <p className="login-page__error">{error}</p>}

        <button className="login-page__submit" type="submit" disabled={loading}>
          {loading ? 'جاري الدخول...' : 'دخول'}
        </button>
      </form>
    </div>
  );
}
