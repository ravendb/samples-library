// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}

	// Extend ImportMetaEnv interface for Vite environment variables
	interface ImportMetaEnv {
		readonly VITE_APP_HTTP: string;
	}
}

export {};
