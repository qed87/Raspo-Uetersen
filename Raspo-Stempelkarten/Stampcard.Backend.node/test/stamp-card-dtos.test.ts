import { describe, test } from "node:test";
import assert from "node:assert/strict";
import {
    approveStampsDtoSchema,
    createTeamDtoSchema,
    normalizeName,
    playerDtoSchema,
    stampCardDtoSchema
} from "../src/stamp-card-dtos.js";

function nextYear(): string {
    return `${new Date().getFullYear() + 1}-01-01`;
}

describe("name normalization", () => {
    test("trims, lower-cases and replaces whitespace by hyphens", () => {
        assert.equal(normalizeName("  Raspo   Uetersen\t2017 "), "raspo-uetersen-2017");
    });
});

describe("team input", () => {
    test("coach emails are lower-cased and de-duplicated", () => {
        const team = createTeamDtoSchema.parse({ name: "2017-1", coaches: ["A@raspo.de", "a@raspo.de"] });

        assert.deepEqual(team.coaches, ["a@raspo.de"]);
    });

    test("a blank team name is rejected", () => {
        assert.equal(createTeamDtoSchema.safeParse({ name: "   " }).success, false);
    });
});

describe("player input", () => {
    test("a birthdate in the future is rejected", () => {
        const result = playerDtoSchema.safeParse({ firstName: "Max", lastName: "Muster", birthdate: nextYear() });

        assert.equal(result.success, false);
    });

    test("a birthdate more than 120 years ago is rejected", () => {
        const result = playerDtoSchema.safeParse({ firstName: "Max", lastName: "Muster", birthdate: "1900-01-01" });

        assert.equal(result.success, false);
    });

    test("a birthdate in a non-ISO format is rejected", () => {
        const result = playerDtoSchema.safeParse({ firstName: "Max", lastName: "Muster", birthdate: "04.03.2017" });

        assert.equal(result.success, false);
    });
});

describe("stamp card input", () => {
    test("an accounting year more than 25 years ago is rejected", () => {
        const result = stampCardDtoSchema.safeParse({
            playerId: "00000000-0000-4000-8000-000000000000",
            accountingYear: new Date().getFullYear() - 25
        });

        assert.equal(result.success, false);
    });
});

describe("approval input", () => {
    test("duplicate stamp ids are approved once", () => {
        const id = "00000000-0000-4000-8000-000000000000";

        assert.deepEqual(approveStampsDtoSchema.parse({ stampIds: [id, id] }).stampIds, [id]);
    });

    test("an empty list of stamp ids is rejected", () => {
        assert.equal(approveStampsDtoSchema.safeParse({ stampIds: [] }).success, false);
    });
});
