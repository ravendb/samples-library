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

<div class="author-page">
	{#if loading}
		<div class="loading-card">
			<p>Loading...</p>
		</div>
	{:else if notFound}
		<div class="not-found-card">
			<h1>Author Not Found</h1>
			<p>The author you're looking for doesn't exist or has been removed.</p>
			<a href={resolve('/')} class="back-link">← Back to Home</a>
		</div>
	{:else if error}
		<div class="error-card">
			<h1>Error</h1>
			<p>{error}</p>
			<a href={resolve('/')} class="back-link">← Back to Home</a>
		</div>
	{:else if author}
		<div class="author-card">
			<div class="author-avatar">
				<img
					src="https://api.dicebear.com/9.x/avataaars/svg?seed={encodeURIComponent(author.id)}"
					alt="Author avatar"
					class="avatar-image"
				/>
			</div>
			<div class="author-info">
				<h1>{author.firstName} {author.lastName}</h1>
				<p class="author-meta">
					<span class="label">ID:</span>
					<span class="value">{author.id}</span>
				</p>
			</div>
		</div>
	{/if}
</div>

<style>
	.author-page {
		max-width: 600px;
		margin: 0 auto;
		padding: 40px 24px;
	}

	.loading-card,
	.not-found-card,
	.error-card,
	.author-card {
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
	}

	.loading-card {
		text-align: center;
		color: #6b7280;
	}

	.not-found-card,
	.error-card {
		text-align: center;
	}

	.not-found-card h1,
	.error-card h1 {
		margin-bottom: 12px;
		font-size: 24px;
		font-weight: 600;
		color: #111827;
	}

	.not-found-card p,
	.error-card p {
		margin-bottom: 24px;
		color: #6b7280;
	}

	.error-card p {
		color: #dc2626;
	}

	.back-link {
		display: inline-block;
		color: #2563eb;
		text-decoration: none;
	}

	.back-link:hover {
		text-decoration: underline;
	}

	.author-card {
		display: flex;
		gap: 24px;
		align-items: center;
	}

	.author-avatar {
		flex-shrink: 0;
		width: 120px;
		height: 120px;
		background: #f3f4f6;
		border-radius: 50%;
		overflow: hidden;
	}

	.avatar-image {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.author-info {
		flex: 1;
	}

	.author-info h1 {
		margin-bottom: 16px;
		font-size: 24px;
		font-weight: 600;
		color: #111827;
	}

	.author-meta {
		margin-bottom: 8px;
		font-size: 14px;
	}

	.label {
		color: #6b7280;
		margin-right: 8px;
	}

	.value {
		font-family: monospace;
		color: #111827;
	}
</style>
