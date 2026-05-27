import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";
import "../styles/ProfilePage.css";

function ProfilePage() {

  const navigate = useNavigate();

  const [profile,
    setProfile] =
    useState(null);

  const fetchProfile =
    async () =>
  {
    try
    {
      const response =
        await api.get(
          "/auth/profile"
        );

      setProfile(
        response.data
      );
    }
    catch(error)
    {
      console.log(error);
    }
  };

  useEffect(() =>
  {
    fetchProfile();
  }, []);

  if(!profile)
  {
    return <h2>Loading...</h2>;
  }

  return (
    <div className="profile-container">

      <div className="profile-card">

        <h1>
          My Profile
        </h1>

        <div className="profile-item">
          <strong>Email:</strong>
          {profile.email}
        </div>

        <div className="profile-item">
          <strong>Plan:</strong>

          {
            profile.plan === "Premium"
            ?
            "Premium ⭐"
            :
            "Free"
          }
        </div>

        <div className="profile-item">
          <strong>Member Since:</strong>

          {
            new Date(
              profile.createdAt
            ).toLocaleDateString()
          }
        </div>

        <div className="profile-item">
          <strong>Total URLs:</strong>
          {profile.totalUrls}
        </div>

        <div className="profile-item">
          <strong>Total Clicks:</strong>
          {profile.totalClicks}
        </div>

        <div className="profile-actions">

            <button
                className="dashboard-btn"
                onClick={() => navigate("/")}
            >
                Dashboard
            </button>

            {
                profile.plan === "Free" &&
                (
                <button
                    className="upgrade-btn"
                    onClick={handleUpgrade}
                >
                    Upgrade Premium
                </button>
                )
            }

        </div>

      </div>

    </div>
  );
}

export default ProfilePage;