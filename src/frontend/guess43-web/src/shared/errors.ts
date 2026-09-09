import { ApiError } from './api/client';

/** Maps stable server error codes to friendly, user-facing messages. */
const messages: Record<string, string> = {
  VALIDATION_ERROR: 'Please check the highlighted fields and try again.',
  EMAIL_ALREADY_EXISTS: 'An account with this email already exists.',
  INVALID_CREDENTIALS: 'The email or password is incorrect.',
  INVALID_REFRESH_TOKEN: 'Your session has expired. Please sign in again.',
  GAME_NOT_FOUND: 'That game could not be found.',
  GAME_ALREADY_COMPLETED: 'This game is already finished.',
  GUESS_OUT_OF_RANGE: 'Enter a number between 1 and 43.',
  GAME_NOT_COMPLETED: 'Only finished games can be deleted.',
  CONCURRENCY_CONFLICT: 'Something changed while you were playing. Please retry.',
  INTERNAL_ERROR: 'Something went wrong on our side. Please try again.',
};

export function toFriendlyMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return messages[error.errorCode] ?? error.message ?? 'Something went wrong.';
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'Something went wrong. Please try again.';
}
