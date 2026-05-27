import {
  Routes,
  Route
} from "react-router-dom";

import DashboardPage
from "./pages/DashboardPage";

import LoginPage
from "./pages/LoginPage";

import RegisterPage
from "./pages/RegisterPage";

import ProtectedRoute
from "./components/ProtectedRoute";

import ProfilePage from "./pages/ProfilePage";

function App() {

  return (
    <Routes>

      <Route
        path="/login"
        element={<LoginPage />}
      />

      <Route
        path="/register"
        element={<RegisterPage />}
      />

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/profile"
        element={<ProfilePage />}
      />

    </Routes>
  );
}

export default App;