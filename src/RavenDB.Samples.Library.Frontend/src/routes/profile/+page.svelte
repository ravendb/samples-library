<script lang="ts">
	import { onMount } from 'svelte';
	import { getUserId, getUserAvatarUrl } from '$lib/utils/userId';
	import { getUserProfile, type UserProfile } from '$lib/services/user';
	import TipBox from '$lib/components/TipBox.svelte';

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

<div class="page-container">
	<h1 class="heading-page">Profile</h1>

	<TipBox
		contextText="Your user profile created automatically for the convenience of using the app. You can see your avatar, id and the list of borrowed books."
		ravendbText="Data for this page is retrieved in an efficient way by using .Include when querying for books. This means that the borrowed books are fetched in one request"
	/>

	<div class="card profile-card">
		{#if avatarUrl}
			<img src={avatarUrl} alt="User avatar" class="profile-avatar avatar-round" />
		{/if}
		<div class="profile-info">
			<p class="profile-label text-muted">User ID</p>
			<p class="profile-value text-mono">{userId}</p>
		</div>
	</div>

	<section class="borrowed-section">
		<h2 class="heading-section">Borrowed Books</h2>
		{#if loading}
			<p class="card card-centered loading-state">Loading...</p>
		{:else if error}
			<p class="card card-centered error-state">{error}</p>
		{:else if userProfile && userProfile.borrowed.length > 0}
			<ul class="card borrowed-list">
				{#each userProfile.borrowed as book (book.id)}
					<li class="borrowed-item">
						<span class="book-title">{book.title}</span>
					</li>
				{/each}
			</ul>
		{:else}
			<p class="card card-centered text-muted">No books currently borrowed.</p>
		{/if}
	</section>
</div>

<style>
	.profile-card {
		display: flex;
		align-items: center;
		gap: var(--spacing-6);
		margin-top: var(--spacing-6);
	}

	.profile-avatar {
		width: 80px;
		height: 80px;
	}

	.profile-info {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.profile-label {
		font-size: var(--font-size-sm);
	}

	.profile-value {
		font-size: var(--font-size-sm);
		color: var(--color-gray-900);
	}

	.borrowed-section {
		margin-top: var(--spacing-8);
	}

	.borrowed-list {
		list-style: none;
		padding: 0;
		overflow: hidden;
	}

	.borrowed-item {
		padding: var(--spacing-4) var(--spacing-6);
		border-bottom: 1px solid var(--color-gray-200);
	}

	.borrowed-item:last-child {
		border-bottom: none;
	}

	.book-title {
		font-weight: 500;
		color: var(--color-gray-900);
	}
</style>
