import './StatCard.css';

type StatCardTone = 'default' | 'success' | 'danger' | 'warning';

interface StatCardProps {
  label: string;
  value: string;
  tone?: StatCardTone;
  icon?: string;
}

export default function StatCard({ label, value, tone = 'default', icon }: StatCardProps) {
  return (
    <div className={`stat-card stat-card--${tone}`}>
      {icon && <span className="stat-card__icon" aria-hidden="true">{icon}</span>}
      <div>
        <div className="stat-card__value">{value}</div>
        <div className="stat-card__label">{label}</div>
      </div>
    </div>
  );
}
