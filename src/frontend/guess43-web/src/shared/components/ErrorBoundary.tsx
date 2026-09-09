import { Component, type ErrorInfo, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

/** Catches unexpected rendering errors and shows a recoverable fallback. */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error('Unhandled UI error', error, info);
  }

  private readonly handleReload = () => {
    this.setState({ hasError: false });
    window.location.assign('/');
  };

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="error-boundary" role="alert">
          <h1>Something went wrong</h1>
          <p>An unexpected error occurred. Please try again.</p>
          <button type="button" onClick={this.handleReload}>
            Back to safety
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
