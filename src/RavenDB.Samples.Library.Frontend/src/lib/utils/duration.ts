/**
 * Formats a duration between two dates into a human-readable string.
 * @param fromDate - The start date
 * @param toDate - The end date
 * @returns A human-readable duration string (e.g., "30 seconds", "5 minutes")
 */
export function formatDuration(fromDate: Date, toDate: Date): string {
	const durationMs = toDate.getTime() - fromDate.getTime();
	const durationSeconds = Math.floor(durationMs / 1000);

	if (durationSeconds < 60) {
		return `${durationSeconds} second${durationSeconds !== 1 ? 's' : ''}`;
	} else {
		const minutes = Math.floor(durationSeconds / 60);
		return `${minutes} minute${minutes !== 1 ? 's' : ''}`;
	}
}
