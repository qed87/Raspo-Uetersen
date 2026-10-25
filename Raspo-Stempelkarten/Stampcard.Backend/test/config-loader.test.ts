import { describe, test } from "node:test";
import assert from "node:assert/strict";
import { appConfig } from "../src/config-loader-plugin/index.js";

const requiredSettings = {
    eventstore: { url: "kurrentdb://localhost:2113?tls=false" },
    jwt: {
        issuer: "https://idp.test/realms/raspo",
        audience: "stampcard-dev",
        jwksUri: "https://idp.test/realms/raspo/certs",
        adminRole: "manager",
        accountingRole: "Accounting"
    }
};

describe("configuration", () => {
    test("the club name defaults to rasensport-uetersen-1926", () => {
        const config = appConfig.parse(requiredSettings);

        assert.equal(config.club.name, "rasensport-uetersen-1926");
    });

    test("a configured club name is used", () => {
        const config = appConfig.parse({ ...requiredSettings, club: { name: "fc-test" } });

        assert.equal(config.club.name, "fc-test");
    });
});
