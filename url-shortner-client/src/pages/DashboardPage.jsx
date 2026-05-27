import { useState, useEffect } from "react";
import api from "../api/axios";
import "../styles/DashboardPage.css";
import { QRCodeCanvas } from "qrcode.react";
import { jwtDecode } from "jwt-decode";

function DashboardPage() {
  const [url, setUrl] = useState("");
  const [shortUrl, setShortUrl] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [urls, setUrls] = useState([]);
  const [selectedQR,setSelectedQR] = useState("");
  const [customAlias,setCustomAlias] = useState("");
  const [expiryDate,setExpiryDate] = useState("");
  const token = localStorage.getItem("token");
  const user = token ? jwtDecode(token) : null;
  const plan = user?.Plan || "Free";
  const [searchTerm, setSearchTerm] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [editingUrl, setEditingUrl] = useState(null);
  const [editOriginalUrl, setEditOriginalUrl] = useState("");
  const [editAlias, setEditAlias] = useState("");
  const [editExpiry, setEditExpiry] = useState("");

  const fetchUrls = async () => {
    try {
      const response = await api.get(
      `/url/myurls?page=${page}&pageSize=10`
    );
    setUrls(response.data.data);
    setTotalPages(
      response.data.totalPages
    );
    } catch (error) {
      console.error(error);
    }
  };

  useEffect(() => {
  fetchUrls();
}, [page]);

  const handleLogout = () => {
    localStorage.removeItem("token");
    window.location.href = "/login";
  };
  
  const handleUpgrade = async () => {
    try {

      await api.post(
        "/auth/upgrade"
      );

      alert(
        "Premium activated. Please login again."
      );

      localStorage.removeItem(
        "token"
      );

      window.location.href =
        "/login";

    }
    catch (error) {

      console.log(error);

      setError(
        "Upgrade failed"
      );
    }
  };
  const handleShorten = async (e) => {

        e.preventDefault();

        setMessage("");
        setError("");
        setShortUrl("");

        try {

          const response =
            await api.post(
              "/url/shorten",
              {
                originalUrl: url,

                customAlias:
                  customAlias || null,

                expiresAt:
                  expiryDate || null
              }
            );

          setShortUrl(
            response.data.shortUrl
          );

          setMessage(
            "URL shortened successfully"
          );

          setUrl("");
          setCustomAlias("");
          setExpiryDate("");

          fetchUrls();

        }
        catch (error)
        {
          console.log(error);

          // =========================
          // RATE LIMIT ERROR
          // =========================

          if (
            error.response?.status === 429
          )
          {
            setError(
              "Too many requests. Please wait 1 minute."
            );

            return;
          }

          // =========================
          // NORMAL ERRORS
          // =========================

          setError(
            error.response?.data ||
            "Failed to shorten URL"
          );
        }
      };
      
  const handleDelete = async (id) => {
    const confirmDelete =
      window.confirm(
        "Delete this URL?"
      );

    if (!confirmDelete) return;

    try {
      await api.delete(`/url/${id}`);

      setMessage(
        "URL deleted successfully"
      );

      fetchUrls();
    } catch {
      setError(
        "Failed to delete URL"
      );
    }
  };

  const handleCopy = (url) => {
    navigator.clipboard.writeText(url);
    setMessage("Copied to clipboard");
    };

    const downloadQR = () => {
        const canvas = document.getElementById("qr-code");

        if (!canvas) return;

        const url = canvas.toDataURL("image/png");

        const link = document.createElement("a");

        link.href = url;
        link.download = "qr-code.png";

        link.click();
    };

  const handleAnalytics = async (
    shortCode
  ) => {
    try {
      const response =
        await api.get(
          `/url/analytics/${shortCode}`
        );

      const data = response.data;

      alert(`
Short Code: ${data.shortCode}
Clicks: ${data.clickCount}
Created: ${new Date(
        data.createdAt
      ).toLocaleString()}
      `);
    } catch {
      setError(
        "Failed to load analytics"
      );
    }
  };

  const totalClicks =
    urls.reduce(
      (sum, item) =>
        sum + item.clickCount,
      0
    );

    const filteredUrls =
      urls.filter(
        (url) =>
          url.originalUrl
            .toLowerCase()
            .includes(
              searchTerm.toLowerCase()
            ) ||

          url.shortCode
            .toLowerCase()
            .includes(
              searchTerm.toLowerCase()
            )
      );

      const activeUrls =
      urls.filter(
        (x) =>
          !x.expiresAt ||
          new Date(x.expiresAt) >
            new Date()
      ).length;

      const expiredUrls =
      urls.filter(
        (x) =>
          x.expiresAt &&
          new Date(x.expiresAt) <
            new Date()
      ).length;

      const topUrl =
      urls.length > 0
        ? urls.reduce(
            (prev, current) =>
              prev.clickCount >
              current.clickCount
                ? prev
                : current
          )
        : null;

        const handleUpdate =
          async () => {

            try {

              await api.put(
                `/url/${editingUrl.id}`,
                {
                  originalUrl:
                    editOriginalUrl,

                  customAlias:
                    editAlias,

                  expiresAt:
                    editExpiry || null
                }
              );

              setMessage(
                "Updated successfully"
              );

              setEditingUrl(null);

              fetchUrls();

            }
            catch(error)
            {
              console.log(error);

              setError(
                "Update failed"
              );
            }
          };
  return (
    <div className="dashboard-container">

      <div className="header">

        <div>
          <h1>URL Shortener</h1>

          <p>
            Manage your URLs and analytics
          </p>
        </div>

        <div
          style={{
            display: "flex",
            gap: "10px"
          }}
        >

        <button
          className="logout-btn"
          onClick={() =>
            window.location.href =
              "/profile"
          }
        >
          Profile
        </button>

          {
            plan === "Free" && (
              <button
                className="upgrade-btn"
                onClick={handleUpgrade}
              >
                Upgrade Premium
              </button>
            )
          }

          <button
            className="logout-btn"
            onClick={handleLogout}
          >
            Logout
          </button>

        </div>

      </div>

      <div className="stats">

        <div className="card">
          <h3>Total URLs</h3>
          <h2>{urls.length}</h2>
        </div>

        <div className="card">
          <h3>Total Clicks</h3>
          <h2>{totalClicks}</h2>
        </div>

        <div className="card">
          <h3>Active URLs</h3>
          <h2>{activeUrls}</h2>
        </div>

        <div className="card">
          <h3>Expired URLs</h3>
          <h2>{expiredUrls}</h2>
        </div>

        <div className="card">
          <h3>Current Plan</h3>
          <h2>{plan}</h2>
        </div>

      </div>

      <div className="create-card">

        <h2>
          Create Short URL
        </h2>

        <form
          className="url-form"
          onSubmit={
            handleShorten
          }
        >

          <input
            type="text"
            placeholder="Enter URL"
            value={url}
            onChange={(e) =>
              setUrl(
                e.target.value
              )
            }
          />

          <input
            type="text"
            placeholder="Custom Alias (Premium)"
            value={customAlias}
            onChange={(e)=>
                setCustomAlias(e.target.value)}
          />

            <input
                type="datetime-local"
                value={expiryDate}
                onChange={(e)=>
                    setExpiryDate(e.target.value)}
            />

          <button
            type="submit"
          >
            Shorten
          </button>

        </form>

      </div>

      {message && (
        <div className="success">
          {message}
        </div>
      )}

      {error && (
        <div className="error">
          {error}
        </div>
      )}

      {shortUrl && (
        <div className="short-url-card">

            <h3>Short URL Created</h3>

            <a
            href={shortUrl}
            target="_blank"
            rel="noreferrer"
            >
            {shortUrl}
            </a>

            <div
            style={{
                marginTop: "20px",
            }}
            >
            <QRCodeCanvas
                id="qr-code"
                value={shortUrl}
                size={180}
            />
            </div>

            <button
            className="analytics-btn"
            style={{
                marginTop: "15px",
            }}
            onClick={downloadQR}
            >
            Download QR
            </button>

        </div>
        )}
      <div className="table-card">

        <h2>
          My URLs
        </h2>
          {
          topUrl && (
            <div className="top-url-card">

              <h2>
                Top Performing URL
              </h2>

              <p>
                <strong>Code:</strong>
                {" "}
                {topUrl.shortCode}
              </p>

              <p>
                <strong>Clicks:</strong>
                {" "}
                {topUrl.clickCount}
              </p>

              <p>
                <strong>Original URL:</strong>
                {" "}
                {topUrl.originalUrl}
              </p>

            </div>
          )
        }

        <div className="search-section">
        <input
          type="text"
          placeholder="Search URLs..."
          value={searchTerm}
          onChange={(e) =>
            setSearchTerm(
              e.target.value
            )
          }
          className="search-input"
        />
        <p className="search-count">
          Showing {filteredUrls.length}
          {" "}
          of
          {" "}
          {urls.length}
          {" "}
          URLs
        </p>
      </div>
        <table>

          <thead>
            <tr>
              <th>
                Short Code
              </th>
              <th>
                Original URL
              </th>
              <th>
                Clicks
              </th>
              <th>
                Created
              </th>
              <th>
                Expiry
              </th>
              <th>
                Actions
              </th>
            </tr>
          </thead>

          <tbody>

            {filteredUrls.length > 0 ? (
              filteredUrls.map((url) => (
                <tr
                  key={url.id}
                >
                  <td>
                    {
                      url.shortCode
                    }
                  </td>

                  <td className="url-cell">
                    {
                      url.originalUrl
                    }
                  </td>

                  <td>
                    {
                      url.clickCount
                    }
                  </td>

                  <td>
                    {new Date(
                      url.createdAt
                    ).toLocaleString()}
                  </td>
                  <td>
                  {
                    url.expiresAt
                    ? (
                        new Date(url.expiresAt)
                        < new Date()
                      )
                        ? (
                          <span className="expired">
                            Expired
                          </span>
                        )
                        : (
                          new Date(
                            url.expiresAt
                          ).toLocaleString()
                        )
                    : "Never"
                  }
                  </td>
                  <td>

                    <button
                      className="analytics-btn"
                      onClick={() =>
                        handleAnalytics(
                          url.shortCode
                        )
                      }
                    >
                      Analytics
                    </button>

                    <button
                      className="delete-btn"
                      onClick={() =>
                        handleDelete(
                          url.id
                        )
                      }
                    >
                      Delete
                    </button>

                    <button
                        className="analytics-btn"
                        onClick={() => handleCopy(
                            `${window.location.origin}/${url.shortCode}`
                        )}
                        >
                        Copy
                    </button>

                    <button
                        className="analytics-btn"
                        onClick={() =>
                            setSelectedQR(
                            `${window.location.origin}/${url.shortCode}`
                            )
                        }
                        >
                        Show QR
                    </button>
                    <button
                      className="analytics-btn"
                      onClick={() => {

                        setEditingUrl(url);

                        setEditOriginalUrl(
                          url.originalUrl
                        );

                        setEditAlias(
                          url.shortCode
                        );

                        setEditExpiry(
                          url.expiresAt
                            ?.slice(0,16) || ""
                        );
                      }}
                    >
                      Edit
                  </button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td
                  colSpan="5"
                >
                  No URLs Found
                </td>
              </tr>
            )}

          </tbody>

        </table>

        <div className="pagination">

          <button
            disabled={page === 1}
            onClick={() =>
              setPage(page - 1)
            }
          >
            Previous
          </button>

          <span>
            Page {page} of {totalPages}
          </span>

          <button
            disabled={
              page === totalPages
            }
            onClick={() =>
              setPage(page + 1)
            }
          >
            Next
          </button>

        </div>

      </div>
        
        {selectedQR && (
        <div
        className="qr-modal-overlay"
        onClick={() =>
            setSelectedQR("")
        }
        >

        <div
            className="qr-modal"
            onClick={(e) =>
            e.stopPropagation()
            }
        >

            <h2>
            QR Code
            </h2>

            <QRCodeCanvas
            id="qr-code"
            value={selectedQR}
            size={250}
            />

            <p
            style={{
                marginTop: "15px",
                wordBreak: "break-all",
            }}
            >
            {selectedQR}
            </p>

            <div
            style={{
                display: "flex",
                gap: "10px",
                justifyContent: "center",
                marginTop: "20px",
            }}
            >

            <button
                className="analytics-btn"
                onClick={downloadQR}
            >
                Download
            </button>

            <button
                className="delete-btn"
                onClick={() =>
                setSelectedQR("")
                }
            >
                Close
            </button>

            </div>

        </div>

        </div>
    )
    }

    {
        editingUrl && (

        <div className="modal-overlay">

          <div className="modal">

            <h2>Edit URL</h2>

            <input
              value={editOriginalUrl}
              onChange={(e)=>
                setEditOriginalUrl(
                  e.target.value
                )
              }
            />

            <input
              value={editAlias}
              onChange={(e)=>
                setEditAlias(
                  e.target.value
                )
              }
            />

            <input
              type="datetime-local"
              value={editExpiry}
              onChange={(e)=>
                setEditExpiry(
                  e.target.value
                )
              }
            />

            <button
              onClick={handleUpdate}
            >
              Save
            </button>

            <button
              onClick={() =>
                setEditingUrl(null)
              }
            >
              Cancel
            </button>

          </div>

        </div>

        )}
    </div>
  );
}

export default DashboardPage;