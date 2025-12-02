<script lang="ts">
	import { onMount } from 'svelte';
	import { getUserId, getUserAvatarUrl } from '$lib/utils/userId';
	import { getUserProfile, type UserProfile, type Book } from '$lib/services/user';

	let userId = $state('');
	let avatarUrl = $state('');
	let userProfile = $state<UserProfile | null>(null);
	let loading = $state(true);
	let error = $state<string | null>(null);

	onMount(async () => {
		userId = getUserId();
		avatarUrl = getUserAvatarUrl(userId);

		try {
			userProfile = await getUserProfile();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load profile';
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title>Profile | Library of Ravens</title>
</svelte:head>

<div class="profile-page">
	<h1>Profile</h1>

	<div class="profile-card">
		{#if avatarUrl}
			<img src={avatarUrl} alt="User avatar" class="profile-avatar" />
		{/if}
		<div class="profile-info">
			<p class="profile-label">User ID</p>
			<p class="profile-value">{userId}</p>
		</div>
	</div>

	<section class="borrowed-section">
		<h2>Borrowed Books</h2>
		{#if loading}
			<p class="loading">Loading...</p>
		{:else if error}
			<p class="error">{error}</p>
		{:else if userProfile && userProfile.borrowed.length > 0}
			<ul class="borrowed-list">
				{#each userProfile.borrowed as book}
					<li class="borrowed-item">
						<span class="book-title">{book.title}</span>
					</li>
				{/each}
			</ul>
		{:else}
			<p class="no-books">No books currently borrowed.</p>
		{/if}
	</section>
</div>

<style>
	.profile-page {
		max-width: 600px;
		margin: 0 auto;
		padding: 40px 24px;
	}

	h1 {
		margin-bottom: 24px;
		font-size: 28px;
		font-weight: 600;
	}

	.profile-card {
		display: flex;
		align-items: center;
		gap: 24px;
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
	}

	.profile-avatar {
		width: 80px;
		height: 80px;
		border-radius: 50%;
		background: #f3f4f6;
	}

	.profile-info {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.profile-label {
		font-size: 14px;
		color: #6b7280;
	}

	.profile-value {
		font-family: monospace;
		font-size: 14px;
		color: #111827;
	}

	.borrowed-section {
		margin-top: 32px;
	}

	.borrowed-section h2 {
		margin-bottom: 16px;
		font-size: 20px;
		font-weight: 600;
	}

	.borrowed-list {
		list-style: none;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		overflow: hidden;
	}

	.borrowed-item {
		padding: 16px 24px;
		border-bottom: 1px solid #e5e7eb;
	}

	.borrowed-item:last-child {
		border-bottom: none;
	}

	.book-title {
		font-weight: 500;
		color: #111827;
	}

	.loading,
	.error,
	.no-books {
		padding: 24px;
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		text-align: center;
		color: #6b7280;
	}

	.error {
		color: #dc2626;
	}
</style>
