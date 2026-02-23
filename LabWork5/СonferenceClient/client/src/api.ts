import axios from 'axios';

const API_URL = 'https://localhost:5001/api/v1'; // твой .NET API

export interface LoginResponse {
  accessToken: string;
}

// login
export const login = async (username: string, password: string) => {
  const response = await axios.post(`${API_URL}/auth/login`, { username, password });
  return response.data as LoginResponse;
};

// Conferences
export const getConferences = async (token: string) => {
  const response = await axios.get(`${API_URL}/conferences`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};

// Talks
export const getTalks = async (token: string, conferenceId: number) => {
  const response = await axios.get(`${API_URL}/conferences/${conferenceId}/talks`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};

// Speakers
export const getSpeakers = async () => {
  const response = await axios.get(`${API_URL}/speakers`);
  return response.data;
};