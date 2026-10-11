/**
 * The user identified by the JWT of the current request.
 * Coaches are identified by their email address, admins may access every team
 * and accountants may evaluate the stamp cards of all teams.
 */
export type AuthenticatedUser = {
    email: string,
    isAdmin: boolean,
    isAccountant: boolean
};
