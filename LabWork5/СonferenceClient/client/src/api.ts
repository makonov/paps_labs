// src/api.ts

const API_URL = import.meta.env.REACT_APP_API_URL;

export interface JwtResponse { 
  accessToken: string; 
  role: string;  
}
export interface ApiError { error: string; message: string; }

// --------- AUTH ----------
export async function login(username: string, password: string): Promise<JwtResponse> {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });

  if (!res.ok) throw new Error('Неверный логин или пароль');
  return res.json();
}

// --------- CONFERENCES ----------
export interface Conference { id?: number; name: string; startDate: string; endDate: string; location: string; }

async function handleResponse(res: Response) {
  if (!res.ok) {
    const data: ApiError = await res.json();
    throw new Error(data.message || 'Ошибка API');
  }
  return res.json();
}

export async function getConferences(): Promise<Conference[]> {
  const res = await fetch(`${API_URL}/api/v1/conferences`);
  return handleResponse(res);
}

export async function getConference(id: number): Promise<Conference> {
  const res = await fetch(`${API_URL}/api/v1/conferences/${id}`);
  return handleResponse(res);
}

export async function createConference(conf: Conference, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(conf),
  });
  return handleResponse(res);
}

export async function updateConference(id: number, conf: Conference, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(conf),
  });
  return handleResponse(res);
}

export async function deleteConference(id: number, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences/${id}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) {
    const data: ApiError = await res.json();
    throw new Error(data.message || 'Ошибка при удалении');
  }
}

// --------- SPEAKERS ----------
export interface Speaker { id?: number; name: string; bio?: string; }

export async function getSpeakers(): Promise<Speaker[]> {
  const res = await fetch(`${API_URL}/api/v1/speakers`);
  return handleResponse(res);
}

export async function getSpeaker(id: number): Promise<Speaker> {
  const res = await fetch(`${API_URL}/api/v1/speakers/${id}`);
  return handleResponse(res);
}

export async function createSpeaker(speaker: Speaker, token: string) {
  const res = await fetch(`${API_URL}/api/v1/speakers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(speaker),
  });
  return handleResponse(res);
}

export async function updateSpeaker(id: number, speaker: Speaker, token: string) {
  const res = await fetch(`${API_URL}/api/v1/speakers/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(speaker),
  });
  return handleResponse(res);
}

export async function deleteSpeaker(id: number, token: string) {
  const res = await fetch(`${API_URL}/api/v1/speakers/${id}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) {
    const data: ApiError = await res.json().catch(() => ({ message: 'Ошибка при удалении', error: 'unknown' }));
    throw new Error(data.message);
  }
}

// --------- TALKS ----------
export interface Talk { id?: number; title: string; speakerId: number; startTime: string; room: string; slides?: string; }

export async function getTalks(conferenceId: number): Promise<Talk[]> {
  const res = await fetch(`${API_URL}/api/v1/conferences/${conferenceId}/talks`);
  return handleResponse(res);
}

export async function getTalk(conferenceId: number, talkId: number): Promise<Talk> {
  const res = await fetch(`${API_URL}/api/v1/conferences/${conferenceId}/talks/${talkId}`);
  return handleResponse(res);
}

export async function createTalk(conferenceId: number, talk: Talk, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences/${conferenceId}/talks`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(talk),
  });
  return handleResponse(res);
}

export async function updateTalk(conferenceId: number, talkId: number, talk: Talk, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences/${conferenceId}/talks/${talkId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
    body: JSON.stringify(talk),
  });
  return handleResponse(res);
}

export async function deleteTalk(conferenceId: number, talkId: number, token: string) {
  const res = await fetch(`${API_URL}/api/v1/conferences/${conferenceId}/talks/${talkId}`, {
    method: 'DELETE',
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) {
    const data: ApiError = await res.json().catch(() => ({ message: 'Ошибка при удалении', error: 'unknown' }));
    throw new Error(data.message);
  }
}