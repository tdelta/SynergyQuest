/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

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
