import { useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/axios";
import "../styles/AuthPage.css";

function LoginPage() {

  const [email, setEmail] =
    useState("");

  const [password, setPassword] =
    useState("");

  const [message, setMessage] =
    useState("");

  const [loading, setLoading] =
    useState(false);

  const handleLogin =
    async (e) => {

      e.preventDefault();

      setMessage("");

      try {

        setLoading(true);

        const response =
          await api.post(
            "/auth/login",
            {
              email,
              password
            }
          );

        localStorage.setItem(
          "token",
          response.data.token
        );

        window.location.href =
          "/";

      } catch (error) {

        console.log(error);

        setMessage(
          error.response?.data?.message ||
          "Invalid email or password"
        );

      } finally {

        setLoading(false);
      }
    };

  return (
    <div className="auth-container">

      <div className="auth-card">

        <h1>
          Welcome Back
        </h1>

        <p>
          Login to your account
        </p>

        <form
          onSubmit={handleLogin}
        >

          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) =>
              setEmail(
                e.target.value
              )
            }
            required
          />

          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) =>
              setPassword(
                e.target.value
              )
            }
            required
          />

          <button
            type="submit"
            disabled={loading}
          >
            {
              loading
                ? "Logging in..."
                : "Login"
            }
          </button>

        </form>

        {
          message &&
          <p className="error">
            {message}
          </p>
        }

        <p className="switch-auth">

          Don't have an account?

          <Link to="/register">
            Register
          </Link>

        </p>

      </div>

    </div>
  );
}

export default LoginPage;