const VERSION = "1.3";
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

async function registerPeriodicNewsCheck() {
    const registration = await (navigator as any).serviceWorker.ready;
    try {
      await registration.periodicSync.register('get-latest-news', {
        minInterval: 20*1000 //24 * 60 * 60 * 1000,
      }); 
    } catch {
      console.log('Periodic Sync could not be registered!');
    }
  }


// starting point
// As soon as the Service Worker gets registered, it fires the activate event.
self.addEventListener('activate', function (event: any) {

    console.log('Service Worker v'+VERSION+' activated.')
    registerPeriodicNewsCheck();
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

const fetchAndCacheLatestNews = () => {
    console.log('seny fetch')
}

self.addEventListener('periodicsync', event => {
    if ((event as any).tag == 'get-latest-news') {
      (event as any).waitUntil(fetchAndCacheLatestNews());
    }
  });


// any = FetchEvent
const networkFirst = (evt: any) => {
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