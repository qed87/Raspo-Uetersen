import { randomUUID, type UUID } from "node:crypto";
import type EventEmitter from "node:events";
import { Result, ok, err } from 'neverthrow';
import z from 'zod';
import { stringify } from "node:querystring";

const PlayerSchema = z.object({
    id: z.uuidv4(),
    firstName: z.string(),
    lastName: z.string(),
    birthdate: z.date(),
    birthplace: z.string()
});

export class TeamAggregate {
    constructor(
        private eventBus: EventEmitter, 
        public club: string, 
        public name: string,
        public version: number = 0) {
        this.id = randomUUID();
    }

    id: UUID;
    createdBy: string = "";
    createdOn: Date = new Date();
    lastEditor: string = "";
    lastModified: Date = new Date();
    deleted: boolean = false;
    readonly players: Player[] = [];
    readonly coaches: Coach[] = [];

    addPlayer(firstName: string, lastName: string, birthdate: Date, birthplace: string) : Result<Player, string> {
        if(this.deleted) {
            return err(`Team '${this.name}' is already deleted`);
        }
        
        const newPlayer = new Player(
            randomUUID(), 
            firstName, 
            lastName, 
            birthdate, 
            birthplace);

        const result = PlayerSchema.safeParse(newPlayer);
        if(!result.success) {
            return err(result.error.message);
        }

        if(newPlayer.birtdate > new Date()) {
            return err("The date of birth must not be in the future.");
        }

        const found = this.players.some(player => player.firstName === newPlayer.firstName && 
            player.lastName === newPlayer.lastName && 
            player.birtdate === newPlayer.birtdate && 
            player.birthplace === newPlayer.birthplace)
        if(found){
            return err(`Player '${JSON.stringify(newPlayer)}' already exists`)
        }
        
        this.players.push(newPlayer);
        return ok(newPlayer);
    }
}

export class Coach {
    Email: string = "";
    Issuer: string = "";
    IssuedOn: Date = new Date()
}

export class Player {
    constructor(
        public id: UUID, 
        public firstName: string, 
        public lastName: string,
        public birtdate: Date,
        public birthplace: string) {
    }

    public Active: boolean = true;
}

export class StampCard {
   Id: UUID = "0000-0000-0000-0000-0000";
   PlayerId: UUID = "0000-0000-0000-0000-0000";
   AccountingYear: number = 0;
   Issuer: string = "dbo";
   IssuedOn: Date = new Date();
   Stamps: Stamp[] = [];
   SharedCodes: SharedCode[] = [];
}

export class SharedCode {
    Code: UUID = "0000-0000-0000-0000-0000";
    ShareMode: 'View' | 'Edit' = "View";
}

export class Stamp {
    Id: UUID = "0000-0000-0000-0000-0000";
    Reason: string = "";
    Issuer: string = "";
    IssuedOn: Date = new Date()
}