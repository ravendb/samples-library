<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import {
		getNotifications,
		deleteNotification,
		type Notification
	} from '$lib/services/user';
	import TipBox from '$lib/components/TipBox.svelte';

	let notifications = $state<Notification[]>([]);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let refreshInterval: ReturnType<typeof setInterval> | null = null;

	async function loadNotifications() {
		try {
			error = null;
			notifications = await getNotifications();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load notifications';
		} finally {
			loading = false;
		}
	}

	async function handleDeleteNotification(notificationId: string) {
		try {
			await deleteNotification(notificationId);
			// Remove the notification from the list
			notifications = notifications.filter((n) => n.id !== notificationId);
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to delete notification';
		}
	}

	onMount(async () => {
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
	<title>Notifications | Library of Ravens</title>
</svelte:head>

<div class="page-container">
	<h1 class="heading-page">Notifications</h1>

	<div class="notifications-content">
		<div class="notifications-left">
			<section class="notifications-section">
				{#if loading}
					<p class="card card-centered loading-state">Loading notifications...</p>
				{:else if error}
					<p class="card card-centered error-state">{error}</p>
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
		</div>

		<div class="notifications-right">
			<TipBox
				contextText="A list of all the existing notifications for the user. Allows deleting notifications and performing actions based on the notification content"
				ravendbText="Notifications simulate RavenDB capabilities in regards to integrating with third party systems. Each notification is created using `Azure Storage Queues ETL` (see: [docs](https://docs.ravendb.net/7.1/studio/database/tasks/ongoing-tasks/azure-queue-storage-etl)). It simulates an integration with an external service that is done solely using `RavenDB`. No code needed to be run on your end to publish information about an event."
			/>
		</div>
	</div>
</div>

<style>
	.notifications-content {
		display: flex;
		gap: var(--spacing-6);
		margin-top: var(--spacing-6);
	}

	.notifications-left {
		flex: 1;
	}

	.notifications-right {
		flex: 1;
		display: flex;
		flex-direction: column;
	}

	.notifications-section {
		width: 100%;
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

	@media (max-width: 799px) {
		.notifications-content {
			flex-direction: column;
		}
	}
</style>
