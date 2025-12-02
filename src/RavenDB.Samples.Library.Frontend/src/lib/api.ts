/**
 * API configuration module for managing API endpoint URLs.
 * Uses the BASE_API_HTTP environment variable to determine the base URL.
 */

/**
 * The base URL for API calls.
 * This is the APP_HTTP environment variable value.
 * Falls back to empty string if not set (for relative URLs in same-origin scenarios).
 */
export const API_BASE_URL: string = import.meta.env.BASE_API_HTTP ?? '';

/**
 * Builds a full API URL from the given route.
 * @param route - The API route (e.g., '/books' or 'books')
 * @param baseUrl - Optional base URL override (defaults to API_BASE_URL)
 * @returns The full API URL (e.g., 'http://localhost:5000/api/books')
 */
export function apiUrl(route: string, baseUrl: string = API_BASE_URL): string {
	const normalizedBase = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
	const normalizedRoute = route.startsWith('/') ? route : `/${route}`;
	return `${normalizedBase}/api${normalizedRoute}`;
}
