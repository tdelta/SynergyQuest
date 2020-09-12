import React from 'react';
import './App.css';
import { ReactComponent as Logo } from './gfx/logo_web.svg';
import warningImg from './gfx/sslWarnings/warning.png';
import warningAdvancedButton from './gfx/sslWarnings/advancedButton.png';
import warningProceedButton from './gfx/sslWarnings/proceedButton.png';
import { boundClass } from 'autobind-decorator';
import { Button, Carousel, CarouselItem, Navbar } from 'react-bootstrap';
import {
  BotInfo,
  BrowserInfo,
  detect,
  NodeInfo,
  ReactNativeInfo,
  SearchBotDeviceInfo,
} from 'detect-browser';

interface AppState {
  carouselPage: number;
}

type BrowserDetection =
  | BrowserInfo
  | SearchBotDeviceInfo
  | BotInfo
  | NodeInfo
  | ReactNativeInfo
  | null;

/**
 * Main UI class
 */
@boundClass
class App extends React.Component<{}, AppState> {
  private static readonly initialState: AppState = {
    carouselPage: 0,
  };

  private static readonly minChromeVersion = 85;

  private browser: BrowserDetection = null;
  private renderedPages: number = 0;

  constructor(props: {}) {
    super(props);

    this.state = App.initialState;
  }

  componentDidMount() {
    this.browser = detect();
  }

  isBrowserSupported(): boolean {
    const majorVersion = parseInt(
      this.browser?.version?.substr(0, this.browser?.version?.indexOf('.')) ||
        '0'
    );

    return (
      this.browser?.name === 'chrome' && majorVersion >= App.minChromeVersion
    );
  }

  isAndroidBrowser(): boolean {
    return this.browser?.os === 'Android OS';
  }

  onPrevious() {
    this.setState({
      carouselPage: Math.max(0, this.state.carouselPage - 1),
    });
  }

  onNext() {
    const nextPage = this.state.carouselPage + 1;

    if (nextPage < this.renderedPages) {
      this.setState({
        carouselPage: nextPage,
      });
    } else {
      window.open(`https://${window.location.hostname}:8000`, '_self');
    }
  }

  makePage(content: JSX.Element): JSX.Element {
    return (
      <CarouselItem className='page'>
        <div className='scrollContainer'>
          <div className='scroll'>
            <div className='scrollContent'>{content}</div>
          </div>
        </div>
      </CarouselItem>
    );
  }

  render() {
    const carousel = (
      <Carousel
        slide
        wrap={false}
        controls={false}
        activeIndex={this.state.carouselPage}
        indicators={false}
      >
        {this.makePage(
          <>
            <Logo id='logo' />
            <p style={{ fontSize: '8vw' }}>Welcome to Synergy Quest!</p>
          </>
        )}
        {!this.isBrowserSupported() &&
          this.makePage(
            <>
              <p>
                A modern browser is required to play. We recommend Google Chrome
                version 85+.
              </p>
              <p>
                {this.isAndroidBrowser() && (
                  <a href='https://play.google.com/store/apps/details?id=com.android.chrome'>
                    Click to Install Newest Chrome
                  </a>
                )}
              </p>
              <p>Without Chrome, the game may not work correctly.</p>
            </>
          )}
        {this.makePage(
          <>
            <p>
              <u>Within the next minutes</u> you will encounter a warning
              message which might look like this:
            </p>
            <img src={warningImg} className='warningImg' alt=' ' />
            <p>We will guide you through this.</p>
          </>
        )}
        {this.makePage(
          <>
            This happens, because SynergyQuest runs on your computer and your
            phone does not know whether it can trust your computer.
          </>
        )}
        {this.makePage(
          <>
            <p>
              When the message will appear, first, you will need to click on
              "Advanced":
            </p>
            <img src={warningAdvancedButton} className='warningImg' alt=' ' />
          </>
        )}
        {this.makePage(
          <>
            <p>
              Then click on "Proceed to {window.location.hostname} (unsafe)":
            </p>
            <img src={warningProceedButton} className='warningImg' alt=' ' />
          </>
        )}
        {this.makePage(
          <>
            <p>
              This is only safe, because SynergyQuest runs on your computer.
            </p>
            <p style={{ textDecoration: 'underline', color: 'red' }}>
              In general, NEVER do this with an unknown website! Do not enter
              sensitive information!
            </p>
          </>
        )}
        {this.makePage(
          <>
            <p>
              Ready? Then click "Next". You can look at the instructions again
              by pressing "Previous".
            </p>
            <p>The warning might look a bit different on your phone.</p>
          </>
        )}
      </Carousel>
    );

    this.renderedPages = carousel.props.children.filter((x: any) => x).length;

    return (
      <>
        {carousel}
        <Navbar fixed='bottom' className='min-vh-10 justify-content-between'>
          {this.state.carouselPage > 0 ? (
            <Button
              className='navButton'
              variant={
                this.state.carouselPage === this.renderedPages - 1
                  ? 'primary'
                  : 'secondary'
              }
              onClick={this.onPrevious}
            >
              Previous
            </Button>
          ) : (
            <span />
          )}
          <Button
            className='navButton'
            variant={
              this.state.carouselPage === this.renderedPages - 1
                ? 'danger'
                : 'primary'
            }
            onClick={this.onNext}
          >
            Next
          </Button>
        </Navbar>
      </>
    );
  }
}

export default App;
