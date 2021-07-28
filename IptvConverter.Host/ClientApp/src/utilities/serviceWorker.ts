
const isDevelopment = process.env.NODE_ENV === 'development';

const registerServiceWorker = () => {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker
          .register('/sw.js')
          .then(function () {
            console.log("Service Worker registered.");
          })
          .catch(function(e) {
              console.error(e);
          });
    }
}

export const getBackgroundSyncPermissionStatus = async() => {
  await navigator.serviceWorker.ready;
  const permissionStatus = await navigator.permissions.query({
    name: 'background-sync'
  });

  if(permissionStatus.state === 'denied') {
      return Promise.reject('Permission for background sync not granted');
  }
  else if (permissionStatus.state === 'prompt') {
      return Promise.reject('Permission status for backgorund sync is prompt');
  }
  else {
    return Promise.resolve();
  }
}

const epgGenerationSyncKey = 'generate-epg';
const epgGenerationSyncMinInterval = 10*1000; // 6 * 60 * 60 * 1000; // every 6 hours
export const generateEpgPeriodicSyncIsRegistered = async() => {
  const registration = await navigator.serviceWorker.ready;
  const allRegistrations = await (registration as any).periodicSync.getTags();
  return allRegistrations.includes(epgGenerationSyncKey);
}

export const registerPeriodicEpgGenerationSync = async() => {
  const registration = await navigator.serviceWorker.ready;
  try {
    (registration as any).periodicSync.register(epgGenerationSyncKey, {
      minInterval: epgGenerationSyncMinInterval,
    });
  }
  catch (e) {
    return Promise.reject(e);
  }
}

// only after you install pwa you can register periodic sync
export const registerPeriodicEpgGeneration = async() => {
  try {
    await getBackgroundSyncPermissionStatus();
  } catch(e) {
    return Promise.reject(e);
  }

  if(await generateEpgPeriodicSyncIsRegistered()) {
    return Promise.resolve('Sync already registered');
  }

  try {
    await registerPeriodicEpgGenerationSync();
  } catch (e) {
    return Promise.reject(e);
  }

  return Promise.resolve('Sync registered');
}

export default registerServiceWorker;
