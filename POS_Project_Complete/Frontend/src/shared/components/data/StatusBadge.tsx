import './StatusBadge.css';

type StatusTone = 'success' | 'danger' | 'warning' | 'info' | 'neutral';

interface StatusBadgeProps {
  label: string;
  tone: StatusTone;
}

export default function StatusBadge({ label, tone }: StatusBadgeProps) {
  return <span className={`status-badge status-badge--${tone}`}>{label}</span>;
}
