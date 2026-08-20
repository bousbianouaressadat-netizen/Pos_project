import './LoadingState.css';

export default function LoadingState({ message = 'جاري التحميل...' }: { message?: string }) {
  return (
    <div className="loading-state">
      <div className="loading-state__spinner" aria-hidden="true" />
      <span>{message}</span>
    </div>
  );
}
