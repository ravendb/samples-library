<script lang="ts">
	interface Props {
		contextText?: string;
		ravendbText?: string;
	}

	let { contextText, ravendbText }: Props = $props();
</script>

{#if contextText || ravendbText}
	<div class="tip-box">
		{#if contextText}
			<div class="tip-section tip-context">
				<h3 class="tip-heading">Library Tip</h3>
				<p class="tip-text">{contextText}</p>
			</div>
		{/if}
		{#if ravendbText}
			<div class="tip-section tip-ravendb">
				<h3 class="tip-heading">RavenDB Tip</h3>
				<p class="tip-text">{ravendbText}</p>
			</div>
		{/if}
	</div>
{/if}

<style>
	.tip-box {
		display: flex;
		flex-direction: column;
		gap: var(--spacing-4);
	}

	.tip-section {
		padding: var(--spacing-4);
		border-radius: var(--radius-lg);
		border: 2px solid;
		flex: 1;
	}

	.tip-context {
		background-color: var(--color-orange-100);
		border-color: var(--color-orange-600);
	}

	.tip-ravendb {
		background-color: var(--color-blue-100);
		border-color: var(--color-blue-600);
	}

	.tip-heading {
		margin: 0 0 var(--spacing-2) 0;
		font-size: var(--font-size-base);
		font-weight: 600;
		color: var(--color-gray-900);
	}

	.tip-context .tip-heading {
		color: var(--color-orange-700);
	}

	.tip-ravendb .tip-heading {
		color: var(--color-blue-700);
	}

	.tip-text {
		margin: 0;
		font-size: var(--font-size-sm);
		color: var(--color-gray-900);
		line-height: 1.6;
	}

	/* Side-by-side layout for large screens */
	@media (min-width: 768px) {
		.tip-box {
			flex-direction: row;
		}
	}
</style>
