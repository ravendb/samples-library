<script lang="ts">
	import { base } from '$app/paths';
	import { searchBooksAndAuthors, type SearchResult } from '$lib/services/search';

	let { isOpen = $bindable(false) }: { isOpen: boolean } = $props();

	let query = $state('');
	let results = $state<SearchResult[]>([]);
	let loading = $state(false);
	let inputRef: HTMLInputElement | undefined = $state();

	let debounceTimer: ReturnType<typeof setTimeout>;

	function handleInput() {
		clearTimeout(debounceTimer);
		debounceTimer = setTimeout(async () => {
			if (query.trim()) {
				loading = true;
				results = await searchBooksAndAuthors(query);
				loading = false;
			} else {
				results = [];
			}
		}, 200);
	}

	function close() {
		isOpen = false;
		query = '';
		results = [];
	}

	function handleBackdropClick(e: MouseEvent) {
		if (e.target === e.currentTarget) {
			close();
		}
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') {
			close();
		}
	}

	function resolveLink(link: string): string {
		return `${base}${link}`;
	}

	$effect(() => {
		if (isOpen && inputRef) {
			inputRef.focus();
		}
	});
</script>

{#if isOpen}
	<div
		class="search-modal-backdrop"
		onclick={handleBackdropClick}
		onkeydown={handleKeydown}
		role="dialog"
		aria-modal="true"
		aria-label="Search"
		tabindex="-1"
	>
		<div class="search-modal">
			<div class="search-input-wrapper">
				<svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
					<circle cx="11" cy="11" r="8" />
					<path d="M21 21l-4.35-4.35" />
				</svg>
				<input
					bind:this={inputRef}
					type="text"
					placeholder="Search books and authors..."
					aria-label="Search books and authors"
					bind:value={query}
					oninput={handleInput}
					class="search-input"
				/>
				<kbd class="kbd">ESC</kbd>
			</div>

			<div class="search-results">
				{#if loading}
					<div class="search-loading">Searching...</div>
				{:else if results.length > 0}
					{#each results as result (result.id)}
						<!-- eslint-disable-next-line svelte/no-navigation-without-resolve -->
						<a href={resolveLink(result.link)} class="search-result-item" onclick={close}>
							<img src={result.imageUrl} alt={result.name} class="result-image" />
							<div class="result-info">
								<span class="result-name">{result.name}</span>
								<span class="result-type">{result.type}</span>
							</div>
						</a>
					{/each}
				{:else if query.trim()}
					<div class="search-no-results">No results found for "{query}"</div>
				{:else}
					<div class="search-hint">Start typing to search...</div>
				{/if}
			</div>
		</div>
	</div>
{/if}

<style>
	.search-modal-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.5);
		display: flex;
		justify-content: center;
		align-items: flex-start;
		padding-top: 10vh;
		z-index: 100;
	}

	.search-modal {
		background: var(--color-white);
		border-radius: var(--radius-lg);
		width: 100%;
		max-width: 560px;
		box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
		overflow: hidden;
	}

	.search-input-wrapper {
		display: flex;
		align-items: center;
		padding: var(--spacing-4);
		border-bottom: 1px solid var(--color-gray-200);
		gap: var(--spacing-3);
	}

	.search-icon {
		width: 20px;
		height: 20px;
		stroke-width: 2;
		color: var(--color-gray-400);
		flex-shrink: 0;
	}

	.search-input {
		flex: 1;
		border: none;
		outline: none;
		font-size: var(--font-size-base);
		background: transparent;
	}

	.search-input::placeholder {
		color: var(--color-gray-400);
	}

	.kbd {
		background: var(--color-gray-100);
		border: 1px solid var(--color-gray-200);
		border-radius: var(--radius-sm);
		padding: 2px 6px;
		font-size: var(--font-size-xs);
		color: var(--color-gray-500);
		font-family: monospace;
	}

	.search-results {
		max-height: 400px;
		overflow-y: auto;
	}

	.search-result-item {
		display: flex;
		align-items: center;
		gap: var(--spacing-3);
		padding: var(--spacing-3) var(--spacing-4);
		text-decoration: none;
		color: inherit;
		transition: background-color 0.15s;
	}

	.search-result-item:hover {
		background: var(--color-gray-100);
	}

	.result-image {
		width: 40px;
		height: 40px;
		border-radius: var(--radius-md);
		object-fit: cover;
		background: var(--color-gray-100);
	}

	.result-info {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.result-name {
		font-weight: 500;
		color: var(--color-gray-900);
	}

	.result-type {
		font-size: var(--font-size-xs);
		color: var(--color-gray-500);
		text-transform: capitalize;
	}

	.search-loading,
	.search-no-results,
	.search-hint {
		padding: var(--spacing-6);
		text-align: center;
		color: var(--color-gray-500);
	}
</style>
