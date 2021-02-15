export function copyClipboardByElement(element) {      
    element.select();
    window.document.execCommand("copy");
}
