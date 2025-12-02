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
	<!-- svelte-ignore a11y_no_static_element_interactions -->
	<div class="search-modal-backdrop" onclick={handleBackdropClick} onkeydown={handleKeydown}>
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
		background: white;
		border-radius: 12px;
		width: 100%;
		max-width: 560px;
		box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
		overflow: hidden;
	}

	.search-input-wrapper {
		display: flex;
		align-items: center;
		padding: 16px;
		border-bottom: 1px solid #e5e7eb;
		gap: 12px;
	}

	.search-icon {
		width: 20px;
		height: 20px;
		stroke-width: 2;
		color: #9ca3af;
		flex-shrink: 0;
	}

	.search-input {
		flex: 1;
		border: none;
		outline: none;
		font-size: 16px;
		background: transparent;
	}

	.search-input::placeholder {
		color: #9ca3af;
	}

	.kbd {
		background: #f3f4f6;
		border: 1px solid #e5e7eb;
		border-radius: 4px;
		padding: 2px 6px;
		font-size: 12px;
		color: #6b7280;
		font-family: monospace;
	}

	.search-results {
		max-height: 400px;
		overflow-y: auto;
	}

	.search-result-item {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 12px 16px;
		text-decoration: none;
		color: inherit;
		transition: background-color 0.15s;
	}

	.search-result-item:hover {
		background: #f3f4f6;
	}

	.result-image {
		width: 40px;
		height: 40px;
		border-radius: 8px;
		object-fit: cover;
		background: #f3f4f6;
	}

	.result-info {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.result-name {
		font-weight: 500;
		color: #111827;
	}

	.result-type {
		font-size: 12px;
		color: #6b7280;
		text-transform: capitalize;
	}

	.search-loading,
	.search-no-results,
	.search-hint {
		padding: 24px;
		text-align: center;
		color: #6b7280;
	}
</style>
