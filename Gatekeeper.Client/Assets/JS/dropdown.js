import { createPopper } from '@popperjs/core';

export function addPopup(popcorn, tooltip, direction) {
    createPopper(popcorn, tooltip, {
        placement: direction,
    });
}
