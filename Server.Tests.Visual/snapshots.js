const PercyScript = require('@percy/script');
const config = { widths: [576, 1200] };
const mobileOnlyConfig = { widths: [576] };

PercyScript.run(async (page, percySnapshot) => {
    page
        .on('console', message =>
            console.log(`${message.type().substr(0, 3).toUpperCase()} ${message.text()}`))
        .on('pageerror', ({ message }) => console.log(message))
        .on('response', response =>
            console.log(`${response.status()} ${response.url()}`))
        .on('requestfailed', request =>
            console.log(`${request.failure().errorText} ${request.url()}`))

    await page.goto('http://localhost/');
    // ensure the page has loaded before capturing a snapshot
    await page.waitForSelector('.auth-base-root');

    await testInstaller(page, percySnapshot);
    await testLogin(page, percySnapshot);
    await testSecurity(page, percySnapshot);

    // Single pages
    await testAuthPages(page, percySnapshot);
});

async function testAuthPages(page, percySnapshot) {
    await page.goto('http://localhost/login/mfa');
    await page.waitForSelector('.auth-base-root');
    await percySnapshot('login-mfa', config);

    await page.goto('http://localhost/auth/403');
    await page.waitForSelector('.auth-base-root');
    await percySnapshot('403', config);
}

async function testInstaller(page, percySnapshot) {
    var nextButton = 'button:nth-of-type(2)';
    var previousButton = 'button:nth-of-type(1)';

    await percySnapshot('installer-entry-step', config);
    await page.click(nextButton);
    await percySnapshot('installer-tls-choice-step', config);
    await page.click('#useLetsEncrypt');
    await page.click(nextButton);
    await percySnapshot('installer-letsencrypt-step', config);
    await page.click(previousButton);
    await percySnapshot('installer-tls-choice-step-after-back', config);
    await page.click('#useGatekeeperCertificate');
    await page.click(nextButton);
    await percySnapshot('installer-email-choice-step', config);
    await page.click('#useCustomSmtp');
    await page.click(nextButton);
    await page.type('.form-group:nth-of-type(1) input', 'localhost', { delay: 100 });
    await page.type('.form-group:nth-of-type(2) input', '25', { delay: 100 });
    await page.type('.form-group:nth-of-type(3) input', 'test', { delay: 100 });
    await page.type('.form-group:nth-of-type(4) input', 'test', { delay: 100 });
    await page.type('.form-group:nth-of-type(5) input', 'test@example.com', { delay: 100 });
    await page.click('body');
    await page.click(nextButton);
    await percySnapshot('installer-create-account-step', config);
    await page.type('.form-group:nth-of-type(1) input', 'testuser', { delay: 100 });
    await page.type('.form-group:nth-of-type(2) input', 'ins3cureTestUserPassw0rd!', { delay: 100 });
    await page.type('.form-group:nth-of-type(3) input', 'test@example.com', { delay: 100 });
    await page.keyboard.press('Tab');
    await page.click(nextButton);
    await page.waitForTimeout(5000);
}

async function testLogin(page, percySnapshot) {
    await percySnapshot('login', config);

    // Pre-Login
    await page.type('input[type="text"]', 'testuser', { delay: 100 });
    await page.type('input[type="password"]', 'ins3cureTestUserPassw0rd!', { delay: 100 });
    await page.waitForTimeout(1000);
    await percySnapshot('pre-login', config);

    // Post-Login
    await page.click('button');
    await page.waitForTimeout(1000);
    await percySnapshot('post-login', config);
    await page.click('.sidebar__second a');
    await percySnapshot('mobile-pressed-on-all-apps', mobileOnlyConfig);
}

async function testSecurity(page, percySnapshot) {
    await clickPrimaryNav(page, 2);
    await percySnapshot('security-mobile-nav', mobileOnlyConfig);

    // Security overview page
    await clickSeconaryNav(page, 1);
    await percySnapshot('security-settings-index', config);

    // Sessions page
    await clickPrimaryNav(page, 2);
    await clickSeconaryNav(page, 3);
    await percySnapshot('security-alerts', config);
}

/**
 * @param {*} page 
 * @param {int} number 
 */
async function clickSeconaryNav(page, number) {
    await page.click('.sidebar__second a:nth-of-type(' + number + ')');
}

/**
 * @param {*} page 
 * @param {int} number 
 */
async function clickPrimaryNav(page, number) {
    await page.click('.sidebar__icon:nth-of-type(' + number + ')');
}