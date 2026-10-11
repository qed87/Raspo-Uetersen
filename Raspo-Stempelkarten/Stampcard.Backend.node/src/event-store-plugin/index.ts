import { KurrentDBClient } from '@kurrent/kurrentdb-client';
import { type FastifyInstance } from 'fastify'
import fastifyPlugin from 'fastify-plugin'
import * as stampcard from '../stamp-card-service.js'
import z from 'zod';
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
    try{
        await client.createProjection(
            "all-club-teams", 
            `
                fromCategory('team')
                .when({
                    $init: function () {
                        return {
                            teams: []
                        }
                    },
                    TeamAdded: function (state, event) {
                        if (event.eventType !== "TeamAdded") return state;
                        if (!event.data) return state;
                        log("Event: " + JSON.stringify(event.data));
                        state.teams.push({ "club": event.data.club, "name": event.data.name, "id": event.streamId, "coaches": [] });
                        return state;
                    },
                    TeamUpdated: function (state, event) {
                        if (event.eventType !== "TeamUpdated") return state;
                        if (!event.data) return state;
                        log("Event: " + JSON.stringify(event.data));
                        const index = state.teams.findIndex((team)  => event.streamId === team.id);
                        if (index == -1) return state;
                        state.teams[index].name = event.data.name;
                        return state;
                    },
                    TeamDeleted: function (state, event) {
                        if (event.eventType !== "TeamDeleted") return state;
                        if (!event.data) return state;
                        log("Event: " + JSON.stringify(event.data));
                        const index = state.teams.findIndex((team) => event.streamId === team.id);
                        if (index == -1) return state;
                        state.teams.splice(index, 1);
                        return state;
                    },
                    CoachAdded: function (state, event) {
                        if (event.eventType !== "CoachAdded") return state;
                        if (!event.data) return state;
                        log("Event: " + JSON.stringify(event.data));
                        const index = state.teams.findIndex((team) => event.streamId === team.id);
                        if (index == -1) return state;
                        state.teams[index].coaches.push(event.data.email);
                        return state;
                    },
                    CoachRemoved: function (state, event) {
                        if (event.eventType !== "CoachRemoved") return state;
                        if (!event.data) return state;
                        log("Event: " + JSON.stringify(event.data));
                        const index = state.teams.findIndex((team) => event.streamId === team.id);
                        if (index == -1) return state;
                        var removeCoachIndex = state.teams[index].coaches.indexOf(event.data.email);
                        state.teams[index].coaches.splice(removeCoachIndex, 1);
                        return state;
                    }
                })
                .outputState();
        `);
    } catch(err) {
        fastify.log.warn(err, "Custom projections already exist.");
    }    

    const stampCardService = new stampcard.StampCardService(client, fastify.eventbus, fastify.config.club.name);
    fastify.decorate("stampCardService", stampCardService);
}

// Wrapping a plugin function with fastify-plugin exposes the decorators
// and hooks, declared inside the plugin to the parent scope.
export default fastifyPlugin(fp, "event-store")