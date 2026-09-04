import os
from typing import Optional
from dotenv import load_dotenv

# Load local environment file if available
load_dotenv()

class Settings:
    GOOGLE_CLOUD_PROJECT: str = os.getenv("GOOGLE_CLOUD_PROJECT", "clearcut-agentic-20260901")
    GOOGLE_CLOUD_PROJECT_NUMBER: str = os.getenv("GOOGLE_CLOUD_PROJECT_NUMBER", "328400425249")
    GOOGLE_CLOUD_LOCATION: str = os.getenv("GOOGLE_CLOUD_LOCATION", "us-central1")
    GEMINI_MODEL: str = os.getenv("GEMINI_MODEL", "gemini-3.5-flash")
    DEMO_VIDEO_GCS_URI: str = os.getenv("DEMO_VIDEO_GCS_URI", "gs://clearcut-agentic-20260901-media-328400425249/clearcut-demo.mp4")
    PARALLEL_API_KEY: Optional[str] = os.getenv("PARALLEL_API_KEY")
    
    # Environment mode: "development" or "production"
    ENVIRONMENT: str = os.getenv("ENVIRONMENT", "development")
    
    # Fixture mode: defaults to True for local Phase-1 sandbox development
    USE_FIXTURES: bool = os.getenv("USE_FIXTURES", "True").lower() in ("true", "1", "yes")

    def validate(self):
        # Strict project guard: reject wrong project ID or number to fail closed
        if self.GOOGLE_CLOUD_PROJECT != "clearcut-agentic-20260901":
            raise ValueError(
                f"CRITICAL SECURITY VIOLATION: Google Cloud Project mismatch! "
                f"Expected 'clearcut-agentic-20260901', got '{self.GOOGLE_CLOUD_PROJECT}'."
            )
            
        if self.GOOGLE_CLOUD_PROJECT_NUMBER != "328400425249":
            raise ValueError(
                f"CRITICAL SECURITY VIOLATION: Google Cloud Project Number mismatch! "
                f"Expected '328400425249', got '{self.GOOGLE_CLOUD_PROJECT_NUMBER}'."
            )

        # Strict fail-closed security rule: Production must never allow fixture mode!
        if self.ENVIRONMENT.lower() == "production" and self.USE_FIXTURES:
            raise RuntimeError(
                "CRITICAL SECURITY VIOLATION: Fixture mode is enabled in a production environment. "
                "The system must fail closed."
            )
            
        # If in production/real-mode, validate that essential secret is present
        if not self.USE_FIXTURES:
            if not self.PARALLEL_API_KEY:
                raise ValueError("PARALLEL_API_KEY must be provided when USE_FIXTURES is disabled.")

settings = Settings()
# Execute initial validation on load
settings.validate()
