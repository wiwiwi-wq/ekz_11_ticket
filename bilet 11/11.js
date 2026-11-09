function createDebouncedSearch(fetcherFn, delayMs) {
  let timerId = null;       
  let controller = null;     

  return function search(query, ...rest) {
    clearTimeout(timerId);

    if (controller) {
      controller.abort();
    }

    controller = new AbortController();

    timerId = setTimeout(async () => {
      try {
        const result = await fetcherFn(query, controller.signal);
        return result;
      } catch (err) {
        if (err.name === 'AbortError') return;
        throw err; 
      }
    }, delayMs);
  };
}