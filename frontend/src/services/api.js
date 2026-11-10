import axios from 'axios';

const API_BASE_URL = 'http://localhost:8080';

export const createApiClient = (studentNid) => {
    return axios.create({
        baseURL: API_BASE_URL,
        headers: {
            'X-Student-Nid': studentNid,
            'Content-Type': 'application/json'
        }
    });
};