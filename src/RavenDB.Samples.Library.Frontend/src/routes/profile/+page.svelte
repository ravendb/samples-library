<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { getUserId, getUserAvatarUrl } from '$lib/utils/userId';
	import {
		getUserProfile,
		getNotifications,
		deleteNotification,
		type UserProfile,
		type Notification
	} from '$lib/services/user';
	import TipBox from '$lib/components/TipBox.svelte';

	let userId = $state('');
	let avatarUrl = $state('');
	let userProfile = $state<UserProfile | null>(null);
	let notifications = $state<Notification[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let notificationsLoading = $state(true);
	let notificationsError = $state<string | null>(null);
	let refreshInterval: ReturnType<typeof setInterval> | null = null;

	async function loadNotifications() {
		try {
			notificationsError = null;
			notifications = await getNotifications();
		} catch (e) {
			notificationsError = e instanceof Error ? e.message : 'Failed to load notifications';
		} finally {
			notificationsLoading = false;
		}
	}

	async function handleDeleteNotification(notificationId: string) {
		try {
			await deleteNotification(notificationId);
			// Remove the notification from the list
			notifications = notifications.filter((n) => n.id !== notificationId);
		} catch (e) {
			notificationsError = e instanceof Error ? e.message : 'Failed to delete notification';
		}
	}

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

		// Load notifications initially
		await loadNotifications();

		// Set up auto-refresh every 60 seconds
		refreshInterval = setInterval(loadNotifications, 60000);
	});

	onDestroy(() => {
		if (refreshInterval) {
			clearInterval(refreshInterval);
		}
	});
</script>

<svelte:head>
	<title>Profile | Library of Ravens</title>
</svelte:head>

<div class="page-container">
	<h1 class="heading-page">Profile</h1>

	<div class="profile-content">
		<div class="profile-left">
			<div class="card profile-card">
				{#if avatarUrl}
					<img src={avatarUrl} alt="User avatar" class="profile-avatar avatar-round" />
				{/if}
				<div class="profile-info">
					<p class="profile-label text-muted">User ID</p>
					<p class="profile-value text-mono">{userId}</p>
				</div>
			</div>
		</div>

		<div class="profile-right">
			<TipBox
				contextText="Your user profile created automatically for the convenience of using the app. You can see your avatar, id and the list of borrowed books."
				ravendbText="Data for this page is retrieved in an efficient way by using `.Include` (see: [docs](https://docs.ravendb.net/7.1/client-api/how-to/handle-document-relationships#includes)) when querying for books. This means that both, the borrowed copies and the books they link to, are fetched in one request."
			/>
		</div>
	</div>

	<section class="notifications-section">
		<h2 class="heading-section">Notifications</h2>
		{#if notificationsLoading}
			<p class="card card-centered loading-state">Loading notifications...</p>
		{:else if notificationsError}
			<p class="card card-centered error-state">{notificationsError}</p>
		{:else if notifications.length > 0}
			<ul class="card notifications-list">
				{#each notifications as notification (notification.id)}
					<li class="notification-item">
						<span class="notification-text">{notification.text}</span>
						<button
							class="notification-delete"
							onclick={() => handleDeleteNotification(notification.id)}
							aria-label="Delete notification"
						>
							×
						</button>
					</li>
				{/each}
			</ul>
		{:else}
			<p class="card card-centered text-muted">No notifications.</p>
		{/if}
	</section>

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
	.profile-content {
		display: flex;
		gap: var(--spacing-6);
	}

	.profile-left {
		flex: 1;
	}

	.profile-right {
		flex: 1;
		display: flex;
		flex-direction: column;
	}

	.profile-card {
		display: flex;
		align-items: center;
		gap: var(--spacing-6);
		height: 100%;
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

	.notifications-section {
		margin-top: var(--spacing-8);
	}

	.notifications-list {
		list-style: none;
		padding: 0;
		overflow: hidden;
	}

	.notification-item {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: var(--spacing-4) var(--spacing-6);
		border-bottom: 1px solid var(--color-gray-200);
		gap: var(--spacing-4);
	}

	.notification-item:last-child {
		border-bottom: none;
	}

	.notification-text {
		flex: 1;
		color: var(--color-gray-900);
	}

	.notification-delete {
		flex-shrink: 0;
		width: 28px;
		height: 28px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: transparent;
		border: 1px solid var(--color-gray-300);
		border-radius: var(--radius-sm);
		color: var(--color-gray-500);
		font-size: 20px;
		line-height: 1;
		cursor: pointer;
		transition: all 0.2s;
	}

	.notification-delete:hover {
		background: var(--color-red-600);
		border-color: var(--color-red-600);
		color: var(--color-white);
	}

	.notification-delete:active {
		transform: scale(0.95);
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

	@media (max-width: 799px) {
		.profile-content {
			flex-direction: column;
		}
	}
</style>
