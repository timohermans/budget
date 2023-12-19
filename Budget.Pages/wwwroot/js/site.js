// enable propovers (https://getbootstrap.com/docs/5.3/components/popovers/#enable-popovers)
function enablePopovers() {
    const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
    const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))
}

document.addEventListener("update-bootstrap", () => {
    enablePopovers();
});

enablePopovers();
