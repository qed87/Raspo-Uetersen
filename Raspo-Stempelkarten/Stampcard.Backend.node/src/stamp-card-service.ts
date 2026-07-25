import { jsonEvent, type EventType, type KurrentDBClient, type ResolvedEvent } from "@kurrent/kurrentdb-client";
import * as model from './stamp-card-model.js'
import * as events from './stamp-card-events.js'
import * as dtos from './stamp-card-dtos.js'
import type EventEmitter from "node:events";
import { randomUUID } from "node:crypto";
import onChange from "on-change";

class StampCardLoader {
    constructor(private readonly eventStoreDb: KurrentDBClient){        
    }

    async loadModel(streamName: string): Promise<model.TeamAggregate> {
        const storedEvents = await this.eventStoreDb.readStream(
            streamName, 
            { 
                direction: 'forwards', 
                fromRevision: 'start' 
            });
            for await (const storedEvent of storedEvents) {
                const eventData = storedEvent;
                if(isTeamAddedEvent(storedEvent)) {

                }
            }
    }
}

function isTeamAddedEvent(resolvedEvent: ResolvedEvent<EventType>): resolvedEvent is ResolvedEvent<events.TeamAdded> {
  return resolvedEvent.type === "TeamAdded";
}

export class StampCardService {    
    private readonly stampCardLoader: StampCardLoader;

    constructor(private readonly eventStoreDb: KurrentDBClient, 
        private readonly eventBus: EventEmitter) {
        this.stampCardLoader = new StampCardLoader(eventStoreDb);
    }

    async createTeam(teamName: string): Promise<boolean> {
        const eventData: events.TeamAdded = {
            club: "Rasensport Uetersen 1926 e.V.",
            name: teamName,
            issuer: "dbo",
            issuedOn: new Date()
        };        

        try{
            await events.teamAddedSchema.parseAsync(eventData);        
            const event = jsonEvent({
                id: randomUUID(),
                type: "TeamAdded",
                data: eventData
            });
            await this.eventStoreDb.appendToStream(`team-${teamName}`, event, { streamState: NO_STREAM });
            return true;
        } catch {
            return false;
        }        
    }

    async addPlayer(teamName: string, firstName: string, lastName: string, birthdate: Date, birhtplace: string) {
        const teamAggregate = await this.stampCardLoader.loadModel(teamName);
        const watchedObject = onChange(teamAggregate, (path, value, prevValue) => {

        });
        
        watchedObject.addPlayer(firstName, lastName, birthdate, birhtplace);        
    }

    async listTeams() : Promise<dtos.TeamReadDto[]> {
        try{
            const teamDto = await this.eventStoreDb.getProjectionState<{ Teams: dtos.TeamReadDto[] }>("all-club-teams");
            return teamDto.Teams;
        } catch {
            return [];
        }
    }
}