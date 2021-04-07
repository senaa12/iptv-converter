
export abstract class BaseClient {
    protected async transformOptions(originalOptions: RequestInit): Promise<RequestInit> {
		try {

			return Promise.resolve({
				...originalOptions
			});
		}
		catch (err) {
			console.warn('base client error');
			console.warn(err);
		}

		return Promise.reject("Error fetching token");
    }

    protected getBaseUrl(defaultUrl: string, baseUrl?: string): string {
		return "https://localhost:44313"
    }
 }