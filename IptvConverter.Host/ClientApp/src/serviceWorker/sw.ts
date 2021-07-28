// watch out for opened tabs
// every time you change the version after hard refresh service worker should be installed again
//if that does not happen than you have it opened in some tab
const VERSION = "1.10";
const CACHE_KEY = `iptvconverter-v-${VERSION}`;

// const OFFLINE_PAGE = '/views/offline.html';

// const PREFETCHED = [
//     '/menu.json',
//     '/intro/start',
//     OFFLINE_PAGE,
//     // [...] other requests
// ];
// const EXCLUDED = ['/manifest.json'];

// async function _tryCache(request: Request, response: Response) {
//     // Only GET requests are allowed to be `put` into the cache.
//     if (request.method.toLowerCase() != 'get')
//         return;
//     const url = request.url;
//     // check if the requested url must not be included in the cache
//     for (var excluded of EXCLUDED) {
//         if (url.endsWith(excluded)) {
//             return;
//         }
//     }
//     // if caching is allowed for this request, then...
//     const cache = await caches.open(CACHE_KEY);
//     cache.put(request, 
//     // responses might be allowed to be used only once, thus cloning 
//     response.clone());
// }

// starting point
// As soon as the Service Worker gets registered, it fires the activate event. (first time only)
self.addEventListener('activate', function (event: any) {
    console.log('Service Worker v'+VERSION+' activated.')

    // In order to clear the cache, then update the {VERSION} above:
    // former cache version, if any, will be cleared.
    // Delete all caches that aren't named {CACHE_KEY}    
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(
                cacheNames.map(function (cacheName) {
                    if (cacheName !== CACHE_KEY) {
                        console.info('Removing outdated cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// any = FetchEvent
const networkFirst = (evt: any) => {
    console.log(`Hello from service worker ${VERSION}`);

    // Network first (refreshes the cache), falling back to cache if offline.
    evt.respondWith(
        caches.match(evt.request)
            .then(
                async (cachedResponse: Response) => {
                    const url = evt.request.url;
                    try {
                        const r = await fetch(evt.request);
                        // might respond `200 ok` even if offline, depending on the browser HTTP cache.
                        console.log('fresh response for ' + url);
                        // await _tryCache(evt.request, r);
                        console.log('try cache')
                        return r;
                    } catch (_) {
                        console.warn(_);
                        if (cachedResponse) {
                            console.log('cached response for ' + url);
                            return cachedResponse;
                        } 
                        // else {
                        //     return caches.match(OFFLINE_PAGE);
                        // }
                    }

                }
            )
    );
}

self.addEventListener('fetch', networkFirst);

const getBaseUrl = (): string => {
    // return "https://localhost:44313";
    return "https://iptv-converter.azurewebsites.net/";
}

const generateEpg = async () => {
    await fetch(`${getBaseUrl()}/api/epg/generate?overrideExisting=true`);
}

const epgGenerationSyncKey = 'generate-epg';
self.addEventListener('periodicsync', event => {
    if ((event as any).tag == epgGenerationSyncKey) {
      (event as any).waitUntil(generateEpg());
    }
  });