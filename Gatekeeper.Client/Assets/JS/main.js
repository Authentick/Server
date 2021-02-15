import { addPopup } from './dropdown';
import { copyClipboardByElement } from './clipboard';

window.addPopup = (popcorn, tooltip, direction) => {
  addPopup(popcorn, tooltip, direction);
};

window.submitForm = (identifier) => {
  document.getElementById(identifier).submit();
};

window.copyClipboardByElement = (element) => {
  copyClipboardByElement(element);
};