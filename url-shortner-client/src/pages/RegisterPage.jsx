import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import api from "../api/axios";
import "../styles/AuthPage.css";

function RegisterPage() {
  const navigate = useNavigate();

  const [email, setEmail] =
    useState("");

  const [password, setPassword] =
    useState("");

  const [confirmPassword,
    setConfirmPassword] =
    useState("");

  const [message, setMessage] =
    useState("");

  const [error, setError] =
    useState("");

  const [loading, setLoading] =
    useState(false);

  const handleRegister =
    async (e) => {

      e.preventDefault();

      setMessage("");
      setError("");

      if (
        password !==
        confirmPassword
      ) {
        setError(
          "Passwords do not match"
        );
        return;
      }

      try {

        setLoading(true);

        const response =
          await api.post(
            "/auth/register",
            {
              email,
              password
            }
          );

        setMessage(
          response.data.message ||
          "Registration successful"
        );

        setTimeout(() => {
          navigate("/login");
        }, 1500);

      } catch (err) {

        console.log(err);

        setError(
          err.response?.data?.message ||
          err.response?.data ||
          "Registration failed"
        );

      } finally {

        setLoading(false);
      }
    };

  return (
    <div className="auth-container">

      <div className="auth-card">

        <h1>
          Create Account
        </h1>

        <p>
          Register to start
          shortening URLs
        </p>

        <form
          onSubmit={handleRegister}
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

          <input
            type="password"
            placeholder="Confirm Password"
            value={
              confirmPassword
            }
            onChange={(e) =>
              setConfirmPassword(
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
                ? "Registering..."
                : "Register"
            }
          </button>

        </form>

        {
          message &&
          <p className="success">
            {message}
          </p>
        }

        {
          error &&
          <p className="error">
            {error}
          </p>
        }

        <p className="switch-auth">

          Already have an account?

          <Link to="/login">
            Login
          </Link>

        </p>

      </div>

    </div>
  );
}

export default RegisterPage;