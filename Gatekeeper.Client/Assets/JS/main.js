import { addPopup } from './dropdown';

window.addPopup = (popcorn, tooltip, direction) => {
  addPopup(popcorn, tooltip, direction);
};

window.submitForm = (identifier) => {
  document.getElementById(identifier).submit();
};