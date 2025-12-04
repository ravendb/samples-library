import { page } from '@vitest/browser/context';
import { describe, expect, it } from 'vitest';
import { render } from 'vitest-browser-svelte';
import TipBox from './TipBox.svelte';

describe('TipBox.svelte', () => {
	it('should render both sections when both texts are provided', async () => {
		render(TipBox, {
			props: {
				contextText: 'This is context text',
				ravendbText: 'This is RavenDB text'
			}
		});

		const contextHeading = page.getByRole('heading', { name: 'About This App' });
		const ravendbHeading = page.getByRole('heading', { name: 'RavenDB Features' });

		await expect.element(contextHeading).toBeInTheDocument();
		await expect.element(ravendbHeading).toBeInTheDocument();
	});

	it('should render only context section when only contextText is provided', async () => {
		render(TipBox, {
			props: {
				contextText: 'This is context text'
			}
		});

		const contextHeading = page.getByRole('heading', { name: 'About This App' });
		await expect.element(contextHeading).toBeInTheDocument();

		const ravendbHeading = page.getByRole('heading', { name: 'RavenDB Features', exact: false });
		await expect.element(ravendbHeading).not.toBeInTheDocument();
	});

	it('should render only RavenDB section when only ravendbText is provided', async () => {
		render(TipBox, {
			props: {
				ravendbText: 'This is RavenDB text'
			}
		});

		const ravendbHeading = page.getByRole('heading', { name: 'RavenDB Features' });
		await expect.element(ravendbHeading).toBeInTheDocument();

		const contextHeading = page.getByRole('heading', { name: 'About This App', exact: false });
		await expect.element(contextHeading).not.toBeInTheDocument();
	});

	it('should not render anything when both texts are missing', async () => {
		const { container } = render(TipBox, {
			props: {}
		});

		expect(container.innerHTML).toBe('<!---->');
	});
});
