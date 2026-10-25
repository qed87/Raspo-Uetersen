import { KurrentDBClient } from '@kurrent/kurrentdb-client';
import { type FastifyInstance } from 'fastify'
import fastifyPlugin from 'fastify-plugin'
import * as stampcard from '../stamp-card-service.js'
import _ from 'lodash';

declare module 'fastify' {
  interface FastifyInstance {
    stampCardService: stampcard.StampCardService
  }
}

/**
 * @param {FastifyInstance} fastify
 * @param {Object} options
 */
async function fp(fastify: FastifyInstance, options :any) {
    const connectionStr = fastify.config.eventstore.url;
    const client = KurrentDBClient.connectionString(connectionStr);
    // Every event of a team stream raises its revision, so all events are handled by $any:
    // the concurrency token is the sequence number of the last event, the team data comes from the team events.
    const allClubTeamsQuery = `
                fromCategory('team')
                .when({
                    $init: function () {
                        return {
                            teams: []
                        }
                    },
                    $any: function (state, event) {
                        if (event.eventType === "TeamAdded") {
                            if (!event.data) return state;
                            log("Event: " + JSON.stringify(event.data));
                            state.teams.push({ "club": event.data.club, "name": event.data.name, "id": event.data.id, "streamId": event.streamId, "coaches": [], "concurrencyToken": event.sequenceNumber });
                            return state;
                        }

                        const index = state.teams.findIndex((team) => event.streamId === team.streamId);
                        if (index == -1) return state;
                        state.teams[index].concurrencyToken = event.sequenceNumber;
                        if (!event.data) return state;

                        switch (event.eventType) {
                            case "TeamUpdated":
                                log("Event: " + JSON.stringify(event.data));
                                state.teams[index].name = event.data.name;
                                break;
                            case "TeamDeleted":
                                log("Event: " + JSON.stringify(event.data));
                                state.teams.splice(index, 1);
                                break;
                            case "CoachAdded":
                                log("Event: " + JSON.stringify(event.data));
                                state.teams[index].coaches.push(event.data.email);
                                break;
                            case "CoachRemoved":
                                log("Event: " + JSON.stringify(event.data));
                                var removeCoachIndex = state.teams[index].coaches.indexOf(event.data.email);
                                if (removeCoachIndex != -1) state.teams[index].coaches.splice(removeCoachIndex, 1);
                                break;
                        }
                        return state;
                    }
                })
                .outputState();
        `;
    try {
        await client.createProjection("all-club-teams", allClubTeamsQuery);
    } catch(err) {
        // An existing projection keeps its old query, the reset rebuilds its state with the current one.
        fastify.log.info("Projection all-club-teams already exists, updating and resetting it.");
        await client.updateProjection("all-club-teams", allClubTeamsQuery);
        await client.resetProjection("all-club-teams");
    }

    const stampCardService = new stampcard.StampCardService(client, fastify.config.club.name);
    fastify.decorate("stampCardService", stampCardService);
}

// Wrapping a plugin function with fastify-plugin exposes the decorators
// and hooks, declared inside the plugin to the parent scope.
export default fastifyPlugin(fp, { name: "event-store", dependencies: ["config-loader"] })