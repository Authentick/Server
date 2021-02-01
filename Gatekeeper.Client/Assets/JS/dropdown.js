import { auto, createPopper } from '@popperjs/core';

export function addPopup(popcorn, tooltip, direction) {
    createPopper(popcorn, tooltip, {
        placement: auto,
        modifiers: [
            {
              name: 'offset',
              options: {
                offset: [0, 0],
              },
            },
          ],
    });
}
