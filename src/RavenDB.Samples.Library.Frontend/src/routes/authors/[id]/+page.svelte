<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { getAuthorById, type Author } from '$lib/services/author';

	let author = $state<Author | null>(null);
	let loading = $state(true);
	let notFound = $state(false);
	let error = $state<string | null>(null);

	onMount(async () => {
		const id = page.params.id;

		try {
			author = await getAuthorById(id);
		} catch (e) {
			if (e instanceof Error && e.message.includes('404')) {
				notFound = true;
			} else {
				error = e instanceof Error ? e.message : 'Failed to load author';
			}
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title
		>{author
			? `${author.firstName} ${author.lastName}`
			: notFound
				? 'Author Not Found'
				: 'Loading...'} | Library of Ravens</title
	>
</svelte:head>

<div class="page-container">
	{#if loading}
		<div class="card card-centered loading-state">
			<p>Loading...</p>
		</div>
	{:else if notFound}
		<div class="card card-centered not-found-state">
			<h1>Author Not Found</h1>
			<p>The author you're looking for doesn't exist or has been removed.</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if error}
		<div class="card card-centered error-state">
			<h1>Error</h1>
			<p>{error}</p>
			<a href={resolve('/')} class="link-primary">← Back to Home</a>
		</div>
	{:else if author}
		<div class="card author-card">
			<div class="author-avatar avatar-round">
				<img
					src="https://api.dicebear.com/9.x/avataaars/svg?seed={encodeURIComponent(author.id)}"
					alt="Author avatar"
					class="image-cover"
				/>
			</div>
			<div class="author-info">
				<h1 class="heading-primary">{author.firstName} {author.lastName}</h1>
				<p class="meta-row">
					<span class="meta-label">ID:</span>
					<span class="meta-value">{author.id}</span>
				</p>
			</div>
		</div>
	{/if}
</div>

<style>
	.author-card {
		display: flex;
		gap: var(--spacing-6);
		align-items: center;
	}

	.author-avatar {
		flex-shrink: 0;
		width: 120px;
		height: 120px;
	}

	.author-info {
		flex: 1;
	}
</style>
