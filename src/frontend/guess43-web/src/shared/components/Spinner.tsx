export function Spinner({ label }: { label?: string }) {
  return (
    <div className="spinner" role="status" aria-live="polite">
      <span className="spinner__dot" aria-hidden="true" />
      <span>{label ?? 'Loading…'}</span>
    </div>
  );
}
