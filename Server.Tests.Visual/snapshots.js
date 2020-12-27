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
});

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
    await page.type('input:nth-of-type(1)', 'localhost', {delay: 100});
    await page.type('input:nth-of-type(2)', '25', {delay: 100});
    await page.type('input:nth-of-type(3)', 'test', {delay: 100});
    await page.type('input:nth-of-type(4)', 'test', {delay: 100});
    await page.type('input:nth-of-type(5)', 'test@example.com', {delay: 100});
    await page.click('body');
    await page.click(nextButton);
    await percySnapshot('installer-create-account-step', config);
    await page.type('input:nth-of-type(1)', 'testuser', {delay: 100});
    await page.type('input:nth-of-type(2)', 'ins3cureTestUserPassw0rd!', {delay: 100});
    await page.type('input:nth-of-type(3)', 'test@example.com', {delay: 100});
    await page.keyboard.press('Tab');
    await page.click(nextButton);
    await page.waitForTimeout(5000);
}

async function testLogin(page, percySnapshot) {
    await percySnapshot('login', config);

    // Pre-Login
    await page.type('input[type="text"]', 'testuser', {delay: 100});
    await page.type('input[type="password"]', 'ins3cureTestUserPassw0rd!', {delay: 100});
    await page.waitForTimeout(1000);
    await percySnapshot('pre-login', config);

    // Post-Login
    await page.click('button');
    await page.waitForTimeout(1000);
    await percySnapshot('post-login', config);    
    await page.click('.sidebar__second a');
    await percySnapshot('mobile-pressed-on-all-apps', mobileOnlyConfig);
}

